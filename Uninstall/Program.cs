using System.Management;

static bool UninstallProgram(string programName)
{
    try
    {
        var mos = new ManagementObjectSearcher($"SELECT * FROM Win32_Product WHERE Name = '{programName}'");
        var programs = mos
            .Get()
            .Cast<ManagementObject>()
            .ToList();

        var maxVesrion = programs.Max(p => int.Parse(p["Version"].ToString().Replace(".", "")));

        foreach (var program in programs)
        {
            if (int.Parse(program["Version"].ToString().Replace(".", "")) != maxVesrion)
                program.InvokeMethod("Uninstall", null);
        }

        return false;

    }
    catch
    {
        return false;
    }
}

UninstallProgram("ScreenWorker");