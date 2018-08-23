// <copyright file="PointcloudVisualizer.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.Common {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using Configuration;
    using GoogleARCore;
    using Reconstruction.Data;
    using Reconstruction.Jobs;
    using Reconstruction.Optimization;
    using Unity.Collections;
    using Unity.Jobs;
    using UnityEngine;

    /// <summary>
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour {
        
        private const int k_MaxPointCount = 61440;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject m_VoxelPrefab;
        public GameObject m_SubVoxelPrefab;
        public DebugText m_DebugText;

        private GameObject m_AllVoxels;

        public Reconstructor m_Reconstructor;

        private Mesh m_Mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];

        private float[] m_Confidences;

        public Material m_CustomMaterialRed;
        public Material m_CustomMaterialGreen;
        public Material m_CustomMaterialBlue;
        private Material m_DefaultMaterial;
        public static Material MaterialBlue;

        public DetectedPlaneGenerator m_DetectedPlaneGenerator;
        private GameObject m_HittedVoxel;

        private HashEntry m_TempHashEntry;
        private Touch m_Touch;
        private bool m_ActiveReconstruction = true;
        private bool m_ActiveRenderer = true;

        [HideInInspector]
        public List<Division> Divisions = new List<Division>();
        public int NumberOfPoints = 0;

        private static PointcloudVisualizer _instance;

        public static PointcloudVisualizer Instance { get { return _instance; } }


        public float CONFIDENCE_THRESHOLD = 0.55f;
        public float SCALE = 0.10f;
        public float CAMERA_UPDATE_DISTANCE = 0.1f;
        public float MAX_RECONSTRUCTION_DISTANCE = 4.0f;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }
            MaterialBlue = m_CustomMaterialBlue;
        }

        /// <summary>
        /// Unity start.
        /// </summary>

        public void Start() {
            m_AllVoxels = new GameObject("AllVoxels");
            m_Reconstructor = new Reconstructor();
            m_Reconstructor.Start();
            m_Reconstructor.GetSpatialHashing().SetSubVoxelPrefab(m_SubVoxelPrefab);
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();
            m_Confidences = new float[k_MaxPointCount];

        }

        /// <summary>
        /// Unity update.
        /// </summary>
        /// 
        private Vector4 detected_point = new Vector4();
        private Pose pose = new Pose();
        private Quaternion zeroQuaternion = new Quaternion();
        private RaycastHit hitInfo = new RaycastHit();
        public void Update() {

            UpdateVoxelDistances(Camera.main.transform.position);
            if (!m_ActiveReconstruction) return;

            // Fill in the data to draw the point cloud.
            if (Frame.PointCloud.IsUpdatedThisFrame) {
                // Copy the point cloud points for mesh verticies.
                NumberOfPoints += Frame.PointCloud.PointCount;

                for (int i = 0; i < Frame.PointCloud.PointCount; i++) {

                    UnityEngine.Profiling.Profiler.BeginSample("PointCloud");
                    m_Points[i] = Frame.PointCloud.GetPoint(i);
                    detected_point = Frame.PointCloud.GetPoint(i);

                    int hash = m_Reconstructor.GetSpatialHashing().GetHash(detected_point, m_Reconstructor.GetSpatialHashing().GetScale(), m_Reconstructor.GetSpatialHashing().GetHashTableSize());

                    if (detected_point.w < Constants.CONFIDENCE_THRESHOLD) continue;
                    if (Vector3.Distance(Camera.main.transform.position, detected_point) > m_Reconstructor.GetSpatialHashing().GetMaxReconstructionDistance()) continue;
                    if (!m_Reconstructor.GetSpatialHashing().Contains(hash)) {
                        //Debug.Log("Reconstructor" + ": " + "Point > " + m_Points[i].ToString("G4") + ", NEW Hash: " + hash + ", Size: " + reconstructor.spatialHashing.Size());
                        Vector3 point = Optimization.ScalePoint(detected_point, m_Reconstructor.GetSpatialHashing().GetScale());
                        pose.position = point;
                        pose.rotation = zeroQuaternion;
                        Anchor anchor = Session.CreateAnchor(pose);
                        GameObject voxelObject = Instantiate(m_VoxelPrefab, pose.position, pose.rotation);
                        float voxelSize = 1.00f / m_Reconstructor.GetSpatialHashing().GetScale();
                        voxelObject.GetComponent<Renderer>().enabled = m_ActiveRenderer;
                        voxelObject.transform.localScale = new Vector3(voxelSize, voxelSize, voxelSize);

                        GameObject parentVoxelGameObject = new GameObject("_voxel_" + hash);
                        GameObject lodContainer = new GameObject("_levelOfDetail_0");
                        voxelObject.transform.SetParent(lodContainer.transform);
                        lodContainer.transform.SetParent(anchor.transform);
                        anchor.transform.SetParent(parentVoxelGameObject.transform);
                        parentVoxelGameObject.transform.SetParent(m_AllVoxels.transform);

                        m_TempHashEntry = m_Reconstructor.GetSpatialHashing().CreateHash(detected_point, new Voxel(voxelObject), lodContainer);
                        m_Reconstructor.GetSpatialHashing().AddToTable(m_TempHashEntry);
                    } else {
                        m_TempHashEntry = (HashEntry) m_Reconstructor.GetSpatialHashing().GetFromTable(hash);
                        m_Reconstructor.GetSpatialHashing().AddPointToEntry(detected_point);
                    }
                    UnityEngine.Profiling.Profiler.EndSample();

                    if (DebugText.Instance.UpdateDistances) m_Reconstructor.GetSpatialHashing().UpdateDistanceResolution(m_TempHashEntry);
                    if (!m_ActiveRenderer) {
                        GameObject gameObject = GameObject.Find("_voxel_" + hash);
                        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.enabled = false;
                    }
                    CheckPointCloseToPlanes(m_TempHashEntry);
                }
                if (m_ActiveRenderer) DrawPointCloud();
                else m_Mesh.Clear();
            }

            GetCubeInfoOnTouch();

        }

        private void DrawPointCloud() {
            // Update the mesh indicies array.
            int[] indices = new int[Frame.PointCloud.PointCount];
            for (int i = 0; i < Frame.PointCloud.PointCount; i++) {
                indices[i] = i;
            }

            m_Mesh.Clear();
            m_Mesh.vertices = m_Points;
            m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
        }

        private void GetCubeInfoOnTouch() {
            if (Input.touchCount < 1 || (m_Touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(m_Touch.position);
            if (Physics.Raycast(ray, out hitInfo)) {
                m_HittedVoxel = hitInfo.transform.gameObject;
                int hash = m_Reconstructor.GetSpatialHashing().GetHash(m_HittedVoxel.transform.position, m_Reconstructor.GetSpatialHashing().GetScale(), m_Reconstructor.GetSpatialHashing().GetHashTableSize());
                HashEntry hashEntry = (HashEntry) m_Reconstructor.GetSpatialHashing().GetFromTable(hash);
                string text = m_HittedVoxel.transform.position.ToString("0.000") + " " + hashEntry.weight.ToString("0.00") + " " + hashEntry.distanceToCamera.ToString("0.00");
                m_DebugText.SetVoxelInfo(text);
            }
        }

        public void CheckCurrentPointsWithNewPlane(DetectedPlaneVisualizer newPlane) {
            foreach (DictionaryEntry entry in m_Reconstructor.GetSpatialHashing().GetHashtable()) {
                HashEntry hashEntry = (HashEntry) entry.Value;
                if (hashEntry.remove) continue;
                ChangeVoxelMaterial(newPlane, hashEntry, m_CustomMaterialGreen);
            }
        }

        private void ChangeVoxelMaterial(DetectedPlaneVisualizer newPlane, HashEntry hashEntry, Material material) {
            switch (Optimization.DoRayCastFromHashEntryToPlane(newPlane, hashEntry)) {
                case Optimization.VoxelPlaneCollision.DOWN:
                    //hashEntry.voxel.gameObject.GetComponent<MeshRenderer>().material = m_CustomMaterialRed;
                    m_Reconstructor.GetSpatialHashing().setRemoved(hashEntry.hash);
                    break;
                case Optimization.VoxelPlaneCollision.UP:
                    //hashEntry.voxel.gameObject.GetComponent<MeshRenderer>().material = m_CustomMaterialGreen;
                    m_Reconstructor.GetSpatialHashing().setRemoved(hashEntry.hash);
                    break;
                case Optimization.VoxelPlaneCollision.INSIDE:
                    //hashEntry.voxel.gameObject.GetComponent<MeshRenderer>().material = material;
                    m_Reconstructor.GetSpatialHashing().RemoveCollider(hashEntry);
                    m_Reconstructor.GetSpatialHashing().setRemoved(hashEntry.hash);
                    //m_Reconstructor.GetSpatialHashing().RemoveFromTable(hashEntry.hash);
                    break;
                default:
                    break;
            }
        }

        private void CheckPointCloseToPlanes(HashEntry hashEntry) {
            foreach (DetectedPlaneVisualizer detectedPlane in m_DetectedPlaneGenerator.GetPlanes()) {
                ChangeVoxelMaterial(detectedPlane, hashEntry, m_CustomMaterialBlue);
            }
        }

        public void ClearVoxels() {
            // Destoring childs of AllVoxels
            NumberOfPoints = 0;
            foreach (Transform child in m_AllVoxels.transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void UpdateVoxelDistances(Vector3 position) {

            NativeArray<HashEntry> hashEntries = new NativeArray<HashEntry>(m_Reconstructor.GetSpatialHashing().GetHashtable().Count, Allocator.Temp);
            NativeArray<float> distances = new NativeArray<float>(m_Reconstructor.GetSpatialHashing().GetHashtable().Count, Allocator.Temp);
            hashEntries.CopyFrom(m_Reconstructor.GetSpatialHashing().GetHashArray());

            var CalculateDistancesJob = new CalculateVoxelDistances() {
                HashEntries = hashEntries,
                cameraPosition = position,
                Distances = distances
            };

            JobHandle CalculateDistancesDependency = CalculateDistancesJob.Schedule(hashEntries.Length, 32);
            CalculateDistancesDependency.Complete();

            for (int i = 0; i < hashEntries.Length; i++) {
                m_Reconstructor.GetSpatialHashing().SetDistance(hashEntries[i], distances[i]);
            }

            hashEntries.Dispose();
            distances.Dispose();

            //m_Reconstructor.GetSpatialHashing().UpdateDistanceResolutions();
        }

        public void SetActiveReconstruction(bool value) { this.m_ActiveReconstruction = value; }
        public bool GetActiveReconstruction() { return m_ActiveReconstruction; }

        public void SetActiveRenderer(bool value) {
            this.m_ActiveRenderer = value;
        }

        public bool GetActiveRenderer() {
            return m_ActiveRenderer;
        }

        public void CreateNewLOD() {
            Divisions.Add(new Division(0, 0, 0));
        }

        public Reconstructor GetReconstructor() {
            return m_Reconstructor;
        }

        public List<Division> GetDivisions() {
            return Divisions;
        }

        public void SetDivision(int i, Division division) {
            Divisions[i] = division;
        }

        public void RemoveDivisionByIndex(int i) {
            Divisions.RemoveAt(i);
        }

        public void UpdateResolution(Vector3 position) {
            m_Reconstructor.GetSpatialHashing().UpdateDistanceResolutions();
        }
    }
}
