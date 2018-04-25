using EasyHttp.Http;
using JsonFx.Json;
using JsonFx.Json.Resolvers;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using TeamCitySharp.Connection;
using TeamCitySharp.DomainEntities;

namespace TeamCitySharp.ActionTypes
{
    public class Projects : IProjects
    {
        private readonly ITeamCityCaller m_caller;
        private string m_fields;

        internal Projects(ITeamCityCaller caller)
        {
            m_caller = caller;
        }

        public Projects GetFields(string fields)
        {
            Projects newInstance = (Projects)MemberwiseClone();
            newInstance.m_fields = fields;
            return newInstance;
        }

        public List<Project> All()
        {
            ProjectWrapper projectWrapper = m_caller.Get<ProjectWrapper>(ActionHelper.CreateFieldUrl("/app/rest/projects", m_fields));

            return projectWrapper.Project;
        }

        public Project ByName(string projectLocatorName)
        {
            Project project = m_caller.GetFormat<Project>(ActionHelper.CreateFieldUrl("/app/rest/projects/name:{0}", m_fields),
                                                     projectLocatorName);

            return project;
        }

        public Project ById(string projectLocatorId)
        {
            Project project = m_caller.GetFormat<Project>(ActionHelper.CreateFieldUrl("/app/rest/projects/id:{0}", m_fields),
                                                     projectLocatorId);

            return project;
        }

        public Project Details(Project project)
        {
            return ById(project.Id);
        }

        public Project Create(string projectName)
        {
            return m_caller.Post<Project>(projectName, HttpContentTypes.TextPlain, "/app/rest/projects/",
                                         HttpContentTypes.ApplicationJson);
        }

        public Project Create(string projectName, string sourceId, string projectId = "")
        {
            string id = projectId == "" ? GenerateID(projectName) : projectId;
            string xmlData =
              $"<newProjectDescription name='{projectName}' id='{id}'><parentProject locator='id:{sourceId}'/></newProjectDescription>";
            HttpResponse response = m_caller.Post(xmlData, HttpContentTypes.ApplicationXml, "/app/rest/projects",
                                        HttpContentTypes.ApplicationJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonReader reader =
                  new JsonReader(
                    new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                Project project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }

        public Project Move(string projectId, string destinationId)
        {
            string xmlData = $"<project id='{destinationId}' />";
            string url = $"/app/rest/projects/id:{projectId}/parentProject";
            HttpResponse response = m_caller.Put(xmlData, HttpContentTypes.ApplicationXml, url, HttpContentTypes.ApplicationJson);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonReader reader =
                  new JsonReader(
                    new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                Project project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }

        internal HttpResponse CopyProject(string projectid, string projectName, string newProjectId,
                                          string parentProjectId = "")
        {
            string parentString = "";
            if (parentProjectId != "")
            {
                parentString = $"<parentProject locator='id:{parentProjectId}'/>";
            }

            string xmlData =
            $"<newProjectDescription name='{projectName}' id='{newProjectId}' copyAllAssociatedSettings='true'><sourceProject locator='id:{projectid}'/>{parentString}</newProjectDescription>";
            HttpResponse response = m_caller.Post(xmlData, HttpContentTypes.ApplicationXml, "/app/rest/projects",
                                        HttpContentTypes.ApplicationJson);
            return response;
        }

        public Project Copy(string projectid, string projectName, string newProjectId, string parentProjectId = "")
        {
            HttpResponse response = CopyProject(projectid, projectName, newProjectId, parentProjectId);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonReader reader =
                  new JsonReader(
                    new DataReaderSettings(new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.Lowercase, "-")));
                Project project = reader.Read<Project>(response.RawText);
                return project;
            }
            return new Project();
        }

        public void Delete(string projectName)
        {
            m_caller.DeleteFormat("/app/rest/projects/name:{0}", projectName);
        }

        public void DeleteById(string projectId)
        {
            m_caller.DeleteFormat("/app/rest/projects/id:{0}", projectId);
        }

        public void DeleteProjectParameter(string projectName, string parameterName)
        {
            m_caller.DeleteFormat("/app/rest/projects/name:{0}/parameters/{1}", projectName, parameterName);
        }

        public void SetProjectParameter(string projectName, string settingName, string settingValue)
        {
            m_caller.PutFormat(settingValue, "/app/rest/projects/name:{0}/parameters/{1}", projectName, settingName);
        }

        public string GenerateID(string projectName)
        {
            projectName = Regex.Replace(projectName, @"[^\p{L}\p{N}]+", "");
            return projectName;
        }

        public bool ModifParameters(string buildTypeId, string param, string value)
        {
            string url = $"/app/rest/projects/id:{buildTypeId}/parameters/{param}";

            HttpResponse response = m_caller.Put(value, HttpContentTypes.TextPlain, url, string.Empty);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool ModifSettings(string projectId, string setting, string value)
        {
            string url = $"/app/rest/projects/{projectId}/{setting}";
            HttpResponse response = m_caller.Put(value, HttpContentTypes.TextPlain, url, string.Empty);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public ProjectFeatures GetProjectFeatures(string projectLocatorId)
        {
            ProjectFeatures projectFeatures = m_caller.GetFormat<ProjectFeatures>(ActionHelper.CreateFieldUrl("/app/rest/projects/id:{0}/projectFeatures", m_fields),
              projectLocatorId);

            return projectFeatures;
        }

        public ProjectFeature GetProjectFeatureByProjectFeature(string projectLocatorId, string projectFeatureId)
        {
            ProjectFeature projectFeature = m_caller.GetFormat<ProjectFeature>(ActionHelper.CreateFieldUrl("/app/rest/projects/id:{0}/projectFeatures/id:{1}", m_fields),
              projectLocatorId, projectFeatureId);

            return projectFeature;
        }

        public ProjectFeature CreateProjectFeature(string projectId, ProjectFeature projectFeature)
        {
            JsonWriter writer =
              new JsonWriter(
                new DataWriterSettings(new JsonResolverStrategy()));
            string data = writer.Write(projectFeature);

            return m_caller.PostFormat<ProjectFeature>(data, HttpContentTypes.ApplicationJson,
              HttpContentTypes.ApplicationJson, "/app/rest/projects/id:{0}/projectFeatures",
              projectId);
        }
        public void DeleteProjectFeature(string projectId, string projectFeatureId)
        {
            m_caller.DeleteFormat("/app/rest/projects/id:{0}/projectFeatures/id:{1}", projectId, projectFeatureId);
        }
    }
}
