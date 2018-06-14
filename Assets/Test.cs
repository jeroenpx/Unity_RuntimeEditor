using Battlehub.RTSaveLoad;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        IProjectManager projectManager = Dependencies.ProjectManager;

        ProjectItem projectItem = projectManager.Project.FlattenHierarchy().Where(item => item.Name == "TestGO").FirstOrDefault();
        projectManager.GetOrCreateObjects(new[] { projectItem }, result =>
        {
            Instantiate(result.First().Object);
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
