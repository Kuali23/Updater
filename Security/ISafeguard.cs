using System;

namespace Kuali.Security
{
    public interface ISafeguard
    {
        /// <summary>
        /// Counter for control
        /// </summary>
        public int SpentTime { set; get; }
        public int SalesMade { set; get; }
        public int? IDSucursal { set; get; }
        public DateTime ActivationDate { set; get; }
        public bool ValideGuard();
        public Action BlockAccess { set; get; }
    }
}
