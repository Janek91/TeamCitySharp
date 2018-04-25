using System.Collections.Generic;
using TeamCitySharp.Connection;
using TeamCitySharp.DomainEntities;

namespace TeamCitySharp.ActionTypes
{
    internal class BuildInvestigations : IBuildInvestigations
    {
        private readonly ITeamCityCaller m_caller;
        private string m_fields;

        internal BuildInvestigations(ITeamCityCaller caller)
        {
            m_caller = caller;
        }

        public BuildInvestigations GetFields(string fields)
        {
            BuildInvestigations newInstance = (BuildInvestigations)MemberwiseClone();
            newInstance.m_fields = fields;
            return newInstance;
        }

        #region IBuildInvestigations Members

        public List<Investigation> All()
        {
            string url = ActionHelper.CreateFieldUrl("/app/rest/investigations", m_fields);

            InvestigationWrapper wrapper = m_caller.Get<InvestigationWrapper>(url);

            return wrapper.Investigation;
        }

        public List<Investigation> InvestigationsByBuildTypeId(string buildTypeId)
        {
            List<Investigation> investigationsByBuildTypeId = new List<Investigation>();
            List<Investigation> investigations = All();

            foreach (Investigation investigation in investigations)
            {
                if (investigation.Scope?.BuildTypes != null)
                {
                    foreach (BuildConfig buildType in investigation.Scope.BuildTypes.BuildType)
                    {
                        if (buildType.Id.Equals(buildTypeId))
                        {
                            investigationsByBuildTypeId.Add(investigation);
                        }
                    }
                }
            }

            return investigationsByBuildTypeId;
        }

        #endregion
    }
}
