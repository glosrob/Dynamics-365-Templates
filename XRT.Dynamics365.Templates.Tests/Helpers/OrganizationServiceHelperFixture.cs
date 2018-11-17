using FakeXrmEasy;
using Microsoft.Xrm.Sdk;

namespace XRT.Dynamics365.Templates.Tests.Helpers
{

    public class OrganizationServiceHelperFixture
    {
        public XrmFakedContext Context { get; set; }
        public IOrganizationService Service { get; set; }
        public OrgServiceHelper Helper { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OrganizationServiceHelperFixture()
        {
            Context = new XrmFakedContext();
            Service = Context.GetFakedOrganizationService();
            Helper = new OrgServiceHelper(Service, (string s) => { }, true);
        }
    }
}