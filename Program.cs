using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input)
{
    static Circuit ParseNetwork(JsonElement element, int depth)
    {
        if (depth > 32) throw new ArgumentException("Maximum nesting depth is 32");
        if (element.ValueKind == JsonValueKind.Number)
        {
            double value = element.GetDouble();
            if (!double.IsFinite(value) || value < 1e-6 || value > 1e12)
                throw new ArgumentException("Use resistances from 0.000001 to 1e12 ohms");
            return new Circuit("resistor", value, []);
        }
        var properties = element.EnumerateObject().ToArray();
        if (properties.Length != 1 || properties[0].Name is not ("series" or "parallel"))
            throw new ArgumentException("Each group needs one series or parallel array");
        var children = properties[0].Value.EnumerateArray().Select(child => ParseNetwork(child, depth + 1)).ToArray();
        if (children.Length == 0) throw new ArgumentException("Resistor groups cannot be empty");
        double total = properties[0].Name == "series" ? children.Sum(child => child.Ohms) :
            1 / children.Sum(child => 1 / child.Ohms);
        if (!double.IsFinite(total) || total <= 0) throw new ArgumentException("Equivalent resistance exceeded numerical range");
        return new Circuit(properties[0].Name, total, children);
    }
    static object Rows(Circuit circuit, double volts)
    {
        double amps = volts / circuit.Ohms;
        if (!double.IsFinite(amps)) throw new ArgumentException("Current exceeded numerical range");
        return new { kind = circuit.Kind, ohms = circuit.Ohms, volts, amps,
                     children = circuit.Children.Select(child => Rows(child, circuit.Kind == "series" ? amps * child.Ohms : volts)).ToArray() };
    }
    Circuit network = ParseNetwork(input.GetProperty("network"), 0);
    double supply = input.GetProperty("volts").GetDouble();
    if (!double.IsFinite(supply) || Math.Abs(supply) > 1e6) throw new ArgumentException("Use a finite voltage within ±1e6");
    return new { equivalentOhms = network.Ohms, amps = supply / network.Ohms, branches = Rows(network, supply) };
}

record Circuit(string Kind, double Ohms, Circuit[] Children);
