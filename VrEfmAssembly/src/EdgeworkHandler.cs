public class EdgeworkHandler
{
    public string Batteries = "";
    public string Indicators = "";
    public string Ports = "";
    public string Serial = "";
    public string Other = "";

    public string GetText()
    {
        return $"Batteries: {Batteries}\nIndicators:{Indicators}\nPorts:{Ports}\nSerial #: {Serial}\nOther:{Other}";
    }

    public void Clear()
    {
        Batteries = "";
        Indicators = "";
        Ports = "";
        Serial = "";
        Other = "";
        VrEfmService.instance.UpdateEdgework();
    }
}