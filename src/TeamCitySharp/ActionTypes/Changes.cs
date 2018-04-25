using System.Collections.Generic;
using System.Linq;
using TeamCitySharp.Connection;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace TeamCitySharp.ActionTypes
{
    internal class Changes : IChanges
    {
        private readonly ITeamCityCaller m_caller;
        private string m_fields;

        internal Changes(ITeamCityCaller caller)
        {
            m_caller = caller;
        }

        public Changes GetFields(string fields)
        {
            Changes newInstance = (Changes)MemberwiseClone();
            newInstance.m_fields = fields;
            return newInstance;
        }

        public List<Change> All()
        {
            ChangeWrapper changeWrapper = m_caller.Get<ChangeWrapper>(ActionHelper.CreateFieldUrl("/app/rest/changes", m_fields));

            return changeWrapper.Change;
        }

        public Change ByChangeId(string id)
        {
            Change change = m_caller.GetFormat<Change>(ActionHelper.CreateFieldUrl("/app/rest/changes/id:{0}", m_fields), id);

            return change;
        }

        public List<Change> ByBuildConfigId(string buildConfigId)
        {
            ChangeWrapper changeWrapper =
              m_caller.GetFormat<ChangeWrapper>(ActionHelper.CreateFieldUrl("/app/rest/changes?buildType={0}", m_fields),
                                               buildConfigId);

            return changeWrapper.Change;
        }

        public Change LastChangeDetailByBuildConfigId(string buildConfigId)
        {
            List<Change> changes = ByBuildConfigId(buildConfigId);

            return changes.FirstOrDefault();
        }

        public List<Change> ByLocator(ChangeLocator locator)
        {
            ChangeWrapper changeWrapper = m_caller.GetFormat<ChangeWrapper>("/app/rest/changes?locator={0}", locator);
            if (changeWrapper.Change.Count > 0)
            {
                return changeWrapper.Change;
            }
            return new List<Change>();
        }
    }
}
