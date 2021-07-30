using System.Collections.Generic;

namespace LiveIngestEndToEndTests
{
    public static class Constants
    {
        // From the list of instrument folders in the cycle_21_1 data archive
        public static readonly List<string> InstrumentNames = new()
        {
            "ALF", "ARGUS", "CHIPIR", "CHRONUS", "CRISP", "CRYOLAB", "DETECT1",
            "DETECT2", "DEVA", "EMMA", "EMU", "ENGINX", "EVS", "GEM", "HET",
            "HIFI", "HRPD", "IMAT", "INES", "INTER", "IRIS", "LAD", "LARMOR",
            "LET", "LOQ", "MAPS", "MARI", "MERLIN", "MUSR", "MUSR_SETUP", "MUX",
            "NIMROD", "OFFSPEC", "OSIRIS", "PEARL", "POLARIS", "POLREF",
            "PRESSURE", "PRISMA", "ROTAX", "SANDALS", "SANS2D", "SELAB", "SETUP",
            "SURF", "SXD", "template", "TEST", "TOSCA", "VESUVIO", "WISH", "ZOOM"
        };
    }
}