using System;

namespace GssCommon.Common.Models.Configurator
{
    public enum BomType 
    { 
        EBOM, 
        MBOM, 
        IBOM 
    }

    public class BomItem 
    {
        public string SKU { get; set; }
        public double Qty { get; set; }
        public BomType Category { get; set; }
    }
}
