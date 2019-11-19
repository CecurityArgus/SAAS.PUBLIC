using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    public class Tenants : List<Tenant>
    {
        public Tenants()
        {
            Add(new Tenant { TenantId = "55EC325B-4993-4421-84A6-3312FE7D26CF", SecretKey = "7BD9CDBC-B3C1-47E1-88F4-DEC98A2F7403", APIKey = "4C4505CF-C658-4D69-A2D7-A027DCAE749E" });
            Add(new Tenant { TenantId = "C2D79825-9F57-4C38-8FE5-676FC49D0C93", SecretKey = "B270D88D-342E-49F8-8FB5-5DA1613EF1EE", APIKey = "FA9279AB-1BB3-4F0B-999F-51697CB4031F" });
            Add(new Tenant { TenantId = "6572D148-6E31-405A-8D6E-E69F69A0A2AF", SecretKey = "A520D71F-E8E4-40E4-BC4A-E74F4101BAB8", APIKey = "4837CD06-4A13-462D-88D0-E042F47501FB" });
        }
    }
}
