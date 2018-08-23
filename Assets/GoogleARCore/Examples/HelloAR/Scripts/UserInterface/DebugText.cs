using GoogleARCore.Examples.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using Configuration;

public class DebugText : MonoBehaviour {

    public PointcloudVisualizer m_PointCloudVisualizer;

    private Text m_FPSText;
    private Text m_SizeText;
    private Text m_ModText;
    private Text m_ScaleText;
    private Text m_InfoText;
    private Button m_ClearButton;
    private Button m_OpenDebugButton;
    private Button m_InstantiateObjectButton;
    private Slider m_ModSlider;
    private Slider m_ScaleSlider;
    private Toggle m_ReconstructionToggle;
    private Toggle m_RendererToggle;
    private GameObject m_DebugPanel;
    private Toggle m_DistancesToggle;
    public bool UpdateDistances { get; internal set; }

    public GameObject IntantiatedPrefab;
    private GameObject andyObject;

    private float m_DeltaTime;
    private float m_Fps;
    private bool m_Opened = true;
    private bool m_Instantiated = true;
    public List<int> NumberOfDetails = new List<int>();

    private static DebugText _instance;

    public static DebugText Instance { get { return _instance; } }


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    // Use this for initialization
    void Start() {
        m_FPSText = GameObject.Find("FPS_Text").GetComponent<Text>();
        m_SizeText = GameObject.Find("Size_Text").GetComponent<Text>();
        m_ModText = GameObject.Find("Mod_Text").GetComponent<Text>();
        m_ScaleText = GameObject.Find("Scale_Text").GetComponent<Text>();
        m_InfoText = GameObject.Find("Info_Text").GetComponent<Text>();
        m_ClearButton = GameObject.Find("Clear_Button").GetComponent<Button>();
        m_ModSlider = GameObject.Find("Mod_Slider").GetComponent<Slider>();
        m_ScaleSlider = GameObject.Find("Scale_Slider").GetComponent<Slider>();
        m_ReconstructionToggle = GameObject.Find("Reconstruction_Toggle").GetComponent<Toggle>();
        m_RendererToggle = GameObject.Find("Renderer_Toggle").GetComponent<Toggle>();
        m_DistancesToggle = GameObject.Find("Distances_Toggle").GetComponent<Toggle>();
        m_OpenDebugButton = GameObject.Find("Debug_Button").GetComponent<Button>();
        m_InstantiateObjectButton = GameObject.Find("Instantiate_Object").GetComponent<Button>();
        m_DebugPanel = GameObject.Find("Debug_Panel");


        //Calls the TaskOnClick/TaskWithParameters method when you click the Button
        m_ClearButton.onClick.AddListener(TaskOnClick);
        m_OpenDebugButton.onClick.AddListener(OpenDebugOnClick);
        m_InstantiateObjectButton.onClick.AddListener(InstantiateOnClick);
        m_ModSlider.onValueChanged.AddListener(ModOnValueChanged);
        m_ScaleSlider.onValueChanged.AddListener(ScaleOnValueChanged);
        m_ReconstructionToggle.onValueChanged.AddListener(ReconstructionOnValueChanged);
        m_RendererToggle.onValueChanged.AddListener(RendererOnValueChanged);
        m_DistancesToggle.onValueChanged.AddListener(DistancesOnValueChanged);

        m_ModText.text = "#Points: " + PointcloudVisualizer.Instance.NumberOfPoints;
        m_ScaleText.text = "Scale: " + Constants.SCALE.ToString("0.00");

        foreach (Division d in PointcloudVisualizer.Instance.GetReconstructor().GetSpatialHashing().GetDivisions()) {
            NumberOfDetails.Add(0);
        }
    }

    internal void increaseNumberOfDetails(int i) {
        NumberOfDetails[i] += 1;
        //Debug.Log(NumberOfDetails.ToString());
    }

    private void InstantiateOnClick() {
        if (m_Instantiated) {
            m_Instantiated = false;
            m_InstantiateObjectButton.GetComponentInChildren<Text>().text = "Instantiate";
            Destroy(andyObject);
        } else {
            m_Instantiated = true;
            m_InstantiateObjectButton.GetComponentInChildren<Text>().text = "Destroy";
            Pose pose = new Pose(new Vector3(), new Quaternion());
            // Instantiate Andy model at the hit pose.
            andyObject = Instantiate(IntantiatedPrefab, new Vector3(), new Quaternion());

            // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
            // world evolves.
            var anchor = Session.CreateAnchor(pose);

            // Make Andy model a child of the anchor.
            andyObject.transform.parent = anchor.transform;
        }
    }

    private void OpenDebugOnClick() {
        if (m_Opened) {
            m_OpenDebugButton.GetComponentInChildren<Text>().text = "Open Debug";
            m_DebugPanel.SetActive(false);
            m_Opened = false;
        } else {
            m_OpenDebugButton.GetComponentInChildren<Text>().text = "Close Debug";
            m_DebugPanel.SetActive(true);
            m_Opened = true;
        }
    }

    private void DistancesOnValueChanged(bool arg0) {
        UpdateDistances = arg0;
    }

    // Update is called once per frame
    void Update() {
        m_DeltaTime += (Time.deltaTime - m_DeltaTime) * 0.1f;
        m_Fps = 1.0f / m_DeltaTime;
        m_FPSText.text = "FPS: " + Mathf.Ceil(m_Fps).ToString();
        string lodText = "";

        if (m_Opened) {
            foreach (Division d in PointcloudVisualizer.Instance.GetReconstructor().GetSpatialHashing().GetDivisions()) {
                int counter = 0;
                GameObject[] gos = (GameObject[]) FindObjectsOfType(typeof(GameObject));
                for (int i = 0; i < gos.Length; i++)
                    if (gos[i].name.Contains("_levelOfDetail_" + d.numberOfDivisions + "_"))
                        counter++;
                lodText += counter + " ";
            }
            m_SizeText.text = "HT Size: " + m_PointCloudVisualizer.m_Reconstructor.GetSpatialHashing().Size() + " " + lodText;

            m_ModText.text = "#Points: " + PointcloudVisualizer.Instance.NumberOfPoints;
        }
    }


    private void RendererOnValueChanged(bool value) {
        Renderer[] renderers = GameObject.Find("AllVoxels").GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            r.enabled = value;
        }
        m_PointCloudVisualizer.SetActiveRenderer(value);

    }

    private void ReconstructionOnValueChanged(bool value) {
        m_PointCloudVisualizer.SetActiveReconstruction(value);
    }

    private void ScaleOnValueChanged(float value) {
        TaskOnClick();
        m_PointCloudVisualizer.m_Reconstructor.GetSpatialHashing().SetScale((int) value);
        m_ScaleText.text = "Scale: " + GetScaleInCm().ToString("0.00");
    }

    private void ModOnValueChanged(float value) {
        TaskOnClick();
        m_PointCloudVisualizer.m_Reconstructor.GetSpatialHashing().SetMaxReconstructionDistance((float) value);

        m_ModText.text = "Max: " + value;
    }

    private void TaskOnClick() {
        m_PointCloudVisualizer.m_Reconstructor.GetSpatialHashing().Clear();
        m_PointCloudVisualizer.ClearVoxels();
        if (GameObject.Find("ExternalObjectsContainer") != null)
            foreach (Transform child in GameObject.Find("ExternalObjectsContainer").transform) {
                GameObject.Destroy(child.gameObject);
            }
        DetectedPlaneGenerator.Instance.Clear();
        for (int i = 0; i < NumberOfDetails.Count; i++) {
            NumberOfDetails[i] = 0;
        }
    }


    public void SetVoxelInfo(string text) {
        m_InfoText.text = "Info: " + text;
    }


    private float GetScaleInCm() {
        return (1.00f / m_PointCloudVisualizer.m_Reconstructor.GetSpatialHashing().GetScale());
    }
}

