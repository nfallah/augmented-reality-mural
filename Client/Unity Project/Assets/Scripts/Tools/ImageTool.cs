using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using MixedReality.Toolkit.SpatialManipulation;

public class ImageTool : MonoBehaviour
{
    // API Key and URL for Unsplash
    private string apiKey = "Jpro__yCaA2UTsl2D_aHVgh1P4W76aNyo9GTyFkLQO8Y";
    private string url = "https://api.unsplash.com/search/photos";

    // UI Elements
    public InputField searchInput;
    public Transform resultsContainer;
    public GameObject imageResultPrefab;

    // Start search when search button is clicked
    public void OnSearchButtonClicked()
    {
        string query = searchInput.text;
        StartCoroutine(GetImages(query));
    }

    // Fetch images from Unsplash
    IEnumerator GetImages(string query)
    {
        string requestUrl = $"{url}?query={query}&client_id={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.LogError(request.error);
        }
        else
        {
            UnsplashResponse response = JsonUtility.FromJson<UnsplashResponse>(request.downloadHandler.text);
            DisplayResults(response.results);
        }
    }

    // Display search results
    void DisplayResults(List<Result> results)
    {
        foreach (Transform child in resultsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var result in results)
        {
            StartCoroutine(DownloadImage(result.urls.regular));
        }
    }

    // Download and display individual images
    IEnumerator DownloadImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.LogError(request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            GameObject newImageResult = Instantiate(imageResultPrefab, resultsContainer);
            newImageResult.GetComponent<RawImage>().texture = texture;
            newImageResult.GetComponent<Button>().onClick.AddListener(() => OnImageSelected(texture));
        }
    }

    // Handle image selection and placement in the environment
    void OnImageSelected(Texture2D texture)
    {
        GameObject imageObject = new GameObject("Placed Image");
        Renderer renderer = imageObject.AddComponent<Renderer>();
        renderer.material.mainTexture = texture;

        // Place the object in front of the user
        imageObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;

        // Add MRTK components for interaction
        imageObject.AddComponent<ObjectManipulator>();
        var manipulator = imageObject.AddComponent<ObjectManipulator>();
        manipulator.HostTransform = imageObject.transform;
    }

    [System.Serializable]
    public class UnsplashResponse
    {
        public List<Result> results;
    }

    [System.Serializable]
    public class Result
    {
        public Urls urls;
    }

    [System.Serializable]
    public class Urls
    {
        public string regular;
    }
}
