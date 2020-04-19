using ExitGames.Client.Photon;
using Fordi.Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;
using Network = Fordi.Networking.Network;


namespace Fordi.Annotation
{
    [System.Serializable]
    public class AnnotationSettings
    {
        float minThickness = .002f;
        float maxThickness = .05f;

        private float selectedThickness = .005f;

        public float SelectedThickness
        {
            get
            {
                return selectedThickness;
            }
        }

        public void SetThickness(float sliderValue)
        {
            selectedThickness = minThickness + (maxThickness - minThickness) * sliderValue;
        }
    }

    public interface IAnnotation
    {
        GameObject WhiteBoard { get; }
        Color SelectedColor { get; }
        AnnotationSettings Settings { get; }
        RaycastHit Hit { get; }
        void RemoteStartNewTrail(int remotePlayerId, Color col, float thickness, int trailViewId, int controllingPlayer);
        void RemoteFinishTrail(int remotePlayerId);
        void RemoteDeletePreviousTrail(int remotePlayerId);
        void RemoteStartNewNote(Vector3 startPosition, int remotePlayerId, Color col, float thickness, int trailViewId, int controllingPlayer);

    }

    public class Annotation : MonoBehaviour, IAnnotation
    {
        [HideInInspector]
        public Transform trailOrigin;
        public GameObject trailObject;
        public GameObject AnnotationRootPrefab;
        public List<Trail> trails = new List<Trail>();
        public GameObject m_penPrefab = null;
        private LayerMask whiteboardLayerMask;

        private const string WhiteBoardTag = "Whiteboard";
        private const string WhiteBoardLayer = "Whiteboard";

        private ISettings m_settings;
        private IPlayer m_player;
        private INetwork m_network;

        public GameObject WhiteBoard { get { return whiteboard; } }

        internal float minThickness = .002f;
        internal float maxThickness = .05f;

        public Transform m_pinchTrailAnchorPrefab, m_penTrailAnchorPrefab;
        public Dictionary<int, List<Trail>> remotePlayerTrailStacks = new Dictionary<int, List<Trail>>();
        public Trail currentDefaultTrail;
        public OVRInput.Controller controller = OVRInput.Controller.RTouch;
        public const string TintColorProperty = "_Color";
        public Color selectedColor;
        public Color SelectedColor { get { return selectedColor; } }
       

        public static Annotation instance;
        [HideInInspector]
        public Transform currentAnnotationRoot = null;

        bool pinchOn = false;

        public DrawMode drawMode = DrawMode.AIR;
        private RaycastHit hit;
        public RaycastHit Hit { get { return hit; } }

        private AnnotationSettings settings = null;
        public AnnotationSettings Settings { get { return settings; } }

        private bool newTrailCreationInProgress = false, trailFinishRequestGenerated = false;

        private bool CurrentlyDrawing { get { return trails.Count != 0 && trails[trails.Count - 1].TrailStatus == TrailStatus.DRAWING; } }

        private Transform finishedAnnotationHolder;
        public GameObject whiteboard;

        private GameObject m_pen = null;
        private Transform m_pinchTrailAnchor, m_penTrailAnchor = null;

        //private IEnumerator endSessionEnumerator, startNewTrailEnumerator, deletePreviousEnumerator;

        private void Awake()
        {
            m_player = IOC.Resolve<IPlayer>();
            m_settings = IOC.Resolve<ISettings>();
            m_network = IOC.Resolve<INetwork>();
            finishedAnnotationHolder = transform;
            whiteboard = GameObject.FindGameObjectWithTag(WhiteBoardTag);
            whiteboardLayerMask = whiteboard.layer;
        }

        private void Start()
        {
            settings = new AnnotationSettings();

            selectedColor = currentDefaultTrail.trailRend.material.GetColor(TintColorProperty);
            

            instance = this;
            if (controller == OVRInput.Controller.RTouch)
            {
                trailOrigin = m_player.RightHand;
            }
            else
                trailOrigin = m_player.LeftHand;
        }

        void Update()
        {
            UpdatePinchAndPenStatus();

            if (pinchOn && OVRInput.GetDown(OVRInput.Button.Two, controller))
            {
                if (drawMode == DrawMode.WHITEBOARD)
                    StartNewNote(hit.point);
                else if (!newTrailCreationInProgress)
                    StartCoroutine(StartNewTrail());
            }
            else if (pinchOn && OVRInput.Get(OVRInput.Button.Two, controller) && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                if (drawMode == DrawMode.WHITEBOARD)
                    StartNewNote(hit.point);
                else if (!newTrailCreationInProgress)
                    //StartNewTrail();
                    StartCoroutine(StartNewTrail());
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller) || OVRInput.GetUp(OVRInput.Button.Two, controller) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller))
                FinishTrail();

            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                DeletePreviousTrail();
            }
        }

        private void EnsureGameobjectIntegrity()
        {
            m_pen = Instantiate(m_penPrefab, m_player.RightHand);
            m_pinchTrailAnchor = Instantiate(m_pinchTrailAnchorPrefab, m_player.RightHand);
            m_penTrailAnchor = Instantiate(m_penTrailAnchorPrefab, m_player.RightHand);
            currentDefaultTrail = m_pinchTrailAnchor.GetComponentInChildren<Trail>();
        }

        private void UpdatePinchAndPenStatus()
        {
            bool pinched = false;
            if (Physics.Raycast(trailOrigin.position, -m_pen.transform.up, out hit, 50.0f, whiteboardLayerMask))
            {
                if (!CurrentlyDrawing && PinchOn())
                {
                    pinched = true;
                    drawMode = DrawMode.WHITEBOARD;
                }
            }
            else 
            {
                if (CurrentlyDrawing && trails[trails.Count - 1].DrawMode == DrawMode.WHITEBOARD)
                    FinishTrail();
                drawMode = DrawMode.AIR;
            }

            if (pinched || PinchOn())
            {
                ConfigureAnnotation(drawMode);

                currentDefaultTrail.trailRend.enabled = true;
                pinchOn = true;

                if (currentAnnotationRoot == null || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
                    StartNewSession();
            }
            else if (PinchOff())
            {
                currentDefaultTrail.trailRend.enabled = false;
                pinchOn = false;
                m_pen.SetActive(false);

                if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller))
                    StartCoroutine(EndSession());
            }
        }

        private void ConfigureAnnotation(DrawMode _drawmode)
        {
            if (_drawmode == DrawMode.AIR)
            {
                currentDefaultTrail.transform.SetParent(m_pinchTrailAnchor);
            }
            else
            {
                currentDefaultTrail.transform.SetParent(m_penTrailAnchor);
                m_pen.SetActive(true);
            }

            currentDefaultTrail.transform.localPosition = Vector3.zero;
        }

        private bool PinchOn()
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller) && OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controller) && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
                return true;
            else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller) && OVRInput.GetDown(OVRInput.NearTouch.PrimaryThumbButtons, controller) && !OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
                return true;
            else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller) && OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controller) && OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, controller))
                return true;
            else
                return false;
        }

        private bool PinchOff()
        {
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller) || OVRInput.GetUp(OVRInput.NearTouch.PrimaryThumbButtons, controller) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller))
                return true;
            else
                return false;
        }

        private void StartNewSession()
        {
            
            currentAnnotationRoot = GameObject.Instantiate(AnnotationRootPrefab, finishedAnnotationHolder).transform;
            if (drawMode == DrawMode.WHITEBOARD)
            {
                currentAnnotationRoot.SetParent(whiteboard.transform);
            }
            else
            {
                currentAnnotationRoot.transform.SetParent(finishedAnnotationHolder);
            }
        }

        private IEnumerator EndSession()
        {
            yield return null;
            if (currentAnnotationRoot != null && currentAnnotationRoot.childCount == 0)
            {
                Destroy(currentAnnotationRoot.gameObject);
                yield break;
            }
            CombineSiblings();
        }

        IEnumerator StartNewTrail()
        {
            newTrailCreationInProgress = true;
            currentDefaultTrail.SetupSync();
            if (PhotonNetwork.InRoom)
            {
                object[] content = new object[6];
                content[0] = selectedColor.r;
                content[1] = selectedColor.g;
                content[2] = selectedColor.b;
                content[3] = selectedColor.a;
                content[4] = settings.SelectedThickness;
                content[5] = currentDefaultTrail.PhotonViewId;
                PhotonNetwork.RaiseEvent(Network.trailBegin, content, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
            }
            Debug.LogError("____trailBegin event fired");
            yield return new WaitForSeconds(m_settings.SelectedPreferences.annotationDelay);
            currentDefaultTrail.ActivateDrawing();
            trails.Add(currentDefaultTrail);
            var newTrail = GameObject.Instantiate(trailObject, trailOrigin);

            currentDefaultTrail = newTrail.GetComponent<Trail>();
            currentDefaultTrail.Init();
            newTrailCreationInProgress = false;
            if (trailFinishRequestGenerated)
            {
                FinishTrail();
                trailFinishRequestGenerated = false;
            }
        }

        /// <summary>
        /// This is used only in annotation mode
        /// </summary>
        /// <param name="startPosition"></param>
        void StartNewNote(Vector3 startPosition)
        {
            var newTrail = GameObject.Instantiate(trailObject, whiteboard.transform);
            Trail newTrailComp = newTrail.GetComponent<Trail>();
            newTrailComp.Init();

            currentDefaultTrail.trailRend.enabled = false;

            newTrailComp.ActivateDrawing(startPosition);

            trails.Add(newTrailComp);


            if (PhotonNetwork.InRoom)
            {
                object[] content = new object[7];
                content[0] = selectedColor.r;
                content[1] = selectedColor.g;
                content[2] = selectedColor.b;
                content[3] = selectedColor.a;
                content[4] = startPosition;
                content[5] = settings.SelectedThickness;
                content[6] = newTrailComp.PhotonViewId;

                PhotonNetwork.RaiseEvent(Network.whiteboardNoteBegan, content, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
            }
        }

        void FinishTrail()
        {
            if (newTrailCreationInProgress)
            {
                trailFinishRequestGenerated = true;
                //Debug.LogError("FinishTrail:  newTrailCreationInProgress");
                return;
            }

            if (trails.Count == 0 || trails[trails.Count - 1].TrailStatus == TrailStatus.FINISHED)
            {
                //Debug.LogError("FinishTrail: " + trails.Count);
                if (trails.Count > 0)
                {
                    Debug.LogError(trails[trails.Count - 1].TrailStatus.ToString());
                }
                return;
            }

            if (trails.Count > 0)
                trails[trails.Count - 1].FinishTrail();
            if (pinchOn)
                currentDefaultTrail.trailRend.enabled = true;

            if (trails.Count > 0 && trails[trails.Count - 1].TrailStatus == TrailStatus.FINISHED && PhotonNetwork.InRoom)
            {
                PhotonNetwork.RaiseEvent(Network.trailFinish, null, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
            }
        }

        IEnumerator DeletePrevious()
        {
            if (trails.Count > 0)
            {
                var lastTrail = trails[trails.Count - 1];
                trails.Remove(lastTrail);
                var lastTrailRoot = lastTrail.transform.parent;
                lastTrail.Delete();
                yield return null;
                //if (lastTrailRoot != null && lastTrailRoot.childCount == 0)
                //    Destroy(lastTrailRoot.gameObject);
            }
            yield return null;
        }

        public void DeletePreviousTrail()
        {
            StartCoroutine(DeletePrevious());

            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.RaiseEvent(Network.deletePreviousTrail, null, new RaiseEventOptions() { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
            }

        }

        public void OnReset()
        {
            DeleteAll();

            Photon.Realtime.Player[] currentPlayers = new Photon.Realtime.Player[0];
            if (PhotonNetwork.InRoom)
                currentPlayers = PhotonNetwork.PlayerList;

            foreach (KeyValuePair<int, List<Trail>> entry in remotePlayerTrailStacks)
            {
                if (entry.Key == PhotonNetwork.LocalPlayer.ActorNumber)
                    continue;
                bool playerInRoom = false;
                foreach (var item in currentPlayers)
                {
                    if (item.ActorNumber == entry.Key)
                    {
                        playerInRoom = true;
                        break;
                    }
                }
                if (!playerInRoom)
                {
                    foreach (var item in entry.Value)
                    {
                        item.Delete();
                    }
                    entry.Value.Clear();
                }
            }
        }

 

        public void DeleteAll()
        {
            int trailCount = trails.Count;
            for (int i = 0; i < trailCount; i++)
            {
                DeletePreviousTrail();
            }
        }

        void CombineSiblings()
        {
            return;
            Mesh combinedMesh = new Mesh();
            MeshFilter[] meshFilters = currentAnnotationRoot.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 0)
                return;

            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length - 1];

            Vector3 oldPos = currentAnnotationRoot.position;
            Quaternion oldRot = currentAnnotationRoot.rotation;

            //currentAnnotationRoot.position = Vector3.zero;
            //currentAnnotationRoot.rotation = Quaternion.identity;

            for (int i = 0; i < combineInstances.Length; i++)
            {
                if (meshFilters[i].transform == currentAnnotationRoot)
                {
                    continue;
                }
                combineInstances[i] = new CombineInstance();
                combineInstances[i].subMeshIndex = 0;
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            combinedMesh.CombineMeshes(combineInstances, true, true);

            currentAnnotationRoot.GetComponent<MeshFilter>().sharedMesh = combinedMesh;

            foreach (var item in meshFilters)
            {
                item.gameObject.SetActive(false);
                currentAnnotationRoot.gameObject.SetActive(true);
            }
        }

        public void ChangeTrailColor(Image image)
        {
            currentDefaultTrail.SetColor(image.color);
            selectedColor = image.color;
        }

        public void ChangeTrailThickness(float sliderValue)
        {
            settings.SetThickness(sliderValue);
            currentDefaultTrail.SetThickness(settings.SelectedThickness);
        }

        #region Networking

        public void RemoteStartNewTrail(int remotePlayerId, Color col, float thickness, int trailViewId, int controllingPlayer)
        {
            StartCoroutine(RemoteStartTrailDelayed(remotePlayerId, col, thickness, trailViewId, controllingPlayer));
        }

        private IEnumerator RemoteStartTrailDelayed(int remotePlayerId, Color col, float thickness, int trailViewId, int controllingPlayer)
        {
            var remotePlayer = m_network.GetRemotePlayer(remotePlayerId);
            if (remotePlayer == null)
            {
                Debug.LogError("Remote Player Representative null");
                yield break;
            }

            remotePlayer.currentDefaultTrail.transform.parent = currentAnnotationRoot;
            remotePlayer.currentDefaultTrail.trailRend.enabled = true;
            remotePlayer.currentDefaultTrail.SetupSync(trailViewId, controllingPlayer);
            yield return new WaitForSeconds(m_settings.SelectedPreferences.annotationDelay);
            remotePlayer.currentDefaultTrail.ActivateDrawing(col, remotePlayer, thickness);

            List<Trail> remoteTrails = null;
            if (!remotePlayerTrailStacks.TryGetValue(remotePlayerId, out remoteTrails))
            {
                remoteTrails = new List<Trail>();
                remotePlayerTrailStacks.Add(remotePlayerId, remoteTrails);
            }
            remoteTrails.Add(remotePlayer.currentDefaultTrail);

            Transform remoteTrailOrigin;
            if (remotePlayer.selectedController == OVRInput.Controller.RTouch)
                remoteTrailOrigin = remotePlayer.RightHand;
            else
                remoteTrailOrigin = remotePlayer.LeftHand;

            var newTrail = GameObject.Instantiate(trailObject, remoteTrailOrigin);
            remotePlayer.currentDefaultTrail = newTrail.GetComponent<Trail>();
        }

        public void RemoteStartNewNote(Vector3 startPosition, int remotePlayerId, Color col, float thickness, int trailViewId, int controllingPlayer)
        {
            var remotePlayer = m_network.GetRemotePlayer(remotePlayerId);

            List<Trail> remoteTrails = null;
            if (!remotePlayerTrailStacks.TryGetValue(remotePlayerId, out remoteTrails))
            {
                remoteTrails = new List<Trail>();
                remotePlayerTrailStacks.Add(remotePlayerId, remoteTrails);
            }


            var newTrail = GameObject.Instantiate(trailObject, whiteboard.transform);
            Trail newTrailComp = newTrail.GetComponent<Trail>();
            newTrailComp.ActivateDrawing(remotePlayer, startPosition, col, thickness, trailViewId, controllingPlayer);
            //trails.Add(newTrailComp);
            remoteTrails.Add(newTrailComp);
        }

        public void RemoteFinishTrail(int remotePlayerId)
        {
            List<Trail> remoteTrails = null;
            if (!remotePlayerTrailStacks.TryGetValue(remotePlayerId, out remoteTrails))
            {
                remoteTrails = new List<Trail>();
                remotePlayerTrailStacks.Add(remotePlayerId, remoteTrails);
            }

            if (remoteTrails.Count > 0)
                remoteTrails[remoteTrails.Count - 1].FinishTrail();
          
        }

        public void InitializeRemotePlayerAnnotation(int playerId)
        {
            List<Trail> remoteTrails = null;
            if(!remotePlayerTrailStacks.TryGetValue(playerId, out remoteTrails))
            {
                remoteTrails = new List<Trail>();
                remotePlayerTrailStacks.Add(playerId, remoteTrails);
            }
        }

        public void RemoteDeletePreviousTrail(int playerId)
        {
            StartCoroutine(RemoteDeletePrevious(playerId));
        }

        IEnumerator RemoteDeletePrevious(int remotePlayerId)
        {
            List<Trail> remoteTrails = null;
            if (!remotePlayerTrailStacks.TryGetValue(remotePlayerId, out remoteTrails))
            {
                remoteTrails = new List<Trail>();
                remotePlayerTrailStacks.Add(remotePlayerId, remoteTrails);
            }


            if (remoteTrails.Count > 0)
            {
                var lastTrail = remoteTrails[remoteTrails.Count - 1];
                remoteTrails.Remove(lastTrail);
                var lastTrailRoot = lastTrail.transform.parent;
                lastTrail.Delete();
                yield return null;
                if (lastTrailRoot != null && lastTrailRoot.childCount == 0)
                {
                    Destroy(lastTrailRoot.gameObject);
                }
            }
            yield return null;
        }

        #endregion

    }
}
