using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Fordi.Common;

namespace Fordi.Annotations
{
    public enum TrailStatus
    {
        DORMANT,
        DRAWING,
        FINISHED
    }

    public class WaitForTrajectoryUpdate : CustomYieldInstruction
    {
        private Trail linkedTrail;
        public override bool keepWaiting
        {
            get
            {
                if (linkedTrail == null)
                    return false;
                return !linkedTrail.m_ReceivedUpdate;
            }
        }

        public WaitForTrajectoryUpdate(Trail _linkedTrail)
        {
            linkedTrail = _linkedTrail;
            Debug.Log("Waiting for new trajectory update");
        }
    }

    public class Trail : MonoBehaviour
    {
       

        public TrailRenderer trailRend;

        private DrawMode drawmode = DrawMode.AIR;

        private IAnnotation m_annotation = null;

        private Networking.RemotePlayer controllingPlayer = null;
        private Transform selectedTrailAnchor = null;
        private TrailStatus trailStatus = TrailStatus.DORMANT;

        public TrailStatus TrailStatus { get { return trailStatus; } }
        public DrawMode DrawMode { get { return drawmode; } }

        private PhotonView photonView;
        private AnnotationView photonTransformView;
        private bool local = true;

        public int PhotonViewId {
            get
            {
                if (photonView == null)
                    return -1;
                else
                    return photonView.ViewID;
            }
        }

        //private void Start()
        //{
        //    Init();
        //}

        private void Awake()
        {
            m_annotation = IOC.Resolve<IAnnotation>();
        }

        private void Update()
        {
            //RaycastHit Hit;
            if (drawmode == DrawMode.WHITEBOARD)
            {
                if (controllingPlayer != null)
                {
                    //if (Physics.Raycast(controllingPlayerIdentifier.pen.position, -controllingPlayerIdentifier.pen.transform.up, out Hit, 50.0f, m_annotation.whiteboardLayerMask))
                    //{
                    //    transform.position = Hit.point;
                    //}
                    //else
                    //{
                    //    FinishTrail();
                    //}
                }
                else
                {
                    transform.position = m_annotation.Hit.point;
                }

            }
        }

        #region CONTROL
        public void Init()
        {
            trailRend.material.SetColor(Annotation.TintColorProperty, m_annotation.SelectedColor);
            SetThickness(m_annotation.Settings.SelectedThickness);
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void FinishTrail()
        {
            //print("FinishTrail");
            if (drawmode == DrawMode.AIR)
            {
                transform.SetParent(Annotation.instance.currentAnnotationRoot);
            }
            this.enabled = false;
            trailStatus = TrailStatus.FINISHED;
            if (photonTransformView != null)
                photonTransformView.enabled = false;
            //Destroy(photonTransformView);
            if (photonView != null)
                photonView.enabled = false;
            //Destroy(photonView);
        }

        public void SetColor(Color col)
        {
            trailRend.material.SetColor(Annotation.TintColorProperty, col);
        }

        public void SetThickness(float value)
        {
            trailRend.widthMultiplier = value;
        }

        public void ActivateDrawing()
        {
            trailRend.time = 1000000000000;
            trailStatus = TrailStatus.DRAWING;
            if (PhotonNetwork.InRoom)
            {
                m_SendUpdateEnumerator = SendUpdateEnumerator();
                StartCoroutine(m_SendUpdateEnumerator);
            }
            //SetupSync();
        }

        public void ActivateDrawing(Vector3 startPosition)
        {
            drawmode = DrawMode.WHITEBOARD;
            transform.SetParent(m_annotation.WhiteBoard.transform);
            transform.position = startPosition;
            trailRend.enabled = true;
            trailRend.time = 1000000000000;
            trailStatus = TrailStatus.DRAWING;
            SetupSync();
            if (PhotonNetwork.InRoom)
            {
                m_SendUpdateEnumerator = SendUpdateEnumerator();
                StartCoroutine(m_SendUpdateEnumerator);
            }
        }

        /// <summary>
        /// For remote player
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="col"></param>
        /// <param name="_controllingPlayerId"></param>
        /// <param name="thickness"></param>
        public void ActivateDrawing(Fordi.Networking.RemotePlayer sender, Vector3 startPosition, Color col, float thickness, int viewId, int controllingPlayerId)
        {
            drawmode = DrawMode.WHITEBOARD;
            if (sender.selectedController == OVRInput.Controller.RTouch)
                selectedTrailAnchor = sender.RightHand;
            else
                selectedTrailAnchor = sender.LeftHand;

            controllingPlayer = sender;
            local = false;
            transform.SetParent(m_annotation.WhiteBoard.transform);
            transform.position = startPosition;
            trailRend.enabled = true;
            trailRend.time = 1000000000000;
            SetColor(col);
            SetThickness(thickness);
            trailStatus = TrailStatus.DRAWING;
            SetupSync(viewId, controllingPlayerId);
        }

        /// <summary>
        /// For remote player
        /// </summary>
        /// <param name="col"></param>
        /// <param name="_controllingPlayerId"></param>
        /// <param name="thickness"></param>
        public void ActivateDrawing(Color col, Networking. RemotePlayer _controllingPlayerId, float thickness)
        {
            if (_controllingPlayerId.selectedController == OVRInput.Controller.RTouch)
                selectedTrailAnchor = _controllingPlayerId.RightHand;
            else
                selectedTrailAnchor = _controllingPlayerId.LeftHand;

            controllingPlayer = _controllingPlayerId;
            local = false;
            trailRend.enabled = true;
            trailRend.time = 1000000000000;
            SetColor(col);
            SetThickness(thickness);
            trailStatus = TrailStatus.DRAWING;
            //SetupSync(viewId);
        }
        #endregion

        #region NETWORKING
        public void SetupSync()
        {
            if (PhotonNetwork.InRoom)
            {
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.OwnershipTransfer = OwnershipOption.Takeover;
                //photonView.synchronization = ViewSynchronization.ReliableDeltaCompressed;
                photonView.ViewID = PhotonNetwork.AllocateViewID(false);

                //photonTransformView = gameObject.AddComponent<AnnotationView>();
                //photonTransformView.m_PositionModel.SynchronizeEnabled = true;

                //photonView.ObservedComponents = new List<Component>();
                //photonView.ObservedComponents.Add(photonTransformView);
                //photonView.TransferOwnership(PhotonNetwork.player);
            }
        }

        public void SetupSync(int viewId, int controllingPlayer)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.OwnershipTransfer = OwnershipOption.Takeover;
                //photonView.synchronization = ViewSynchronization.ReliableDeltaCompressed;
                photonView.ViewID = viewId;

                //photonTransformView = gameObject.AddComponent<AnnotationView>();
                //photonTransformView.m_PositionModel.SynchronizeEnabled = true;

                //photonView.ObservedComponents = new List<Component>();
                //photonView.ObservedComponents.Add(photonTransformView);
                //photonView.TransferOwnership(controllingPlayer);
            }
        }

      

        private static int trajectoryUpdateRate = 100; //in milliseconds
        private static int pointsArrayLength = 5;
        private IEnumerator m_DrawEnumerator, m_SendUpdateEnumerator;
        public bool m_ReceivedUpdate = false;
        private Vector3[] m_Points;
        private Vector3[] m_CachedPoints = new Vector3[pointsArrayLength];

        private IEnumerator SendUpdateEnumerator()
        {
            float duration = (float)trajectoryUpdateRate / (1000 * pointsArrayLength);
            while (trailStatus == TrailStatus.DRAWING)
            {
                for (int i = 0; i < m_CachedPoints.Length; i++)
                {
                    m_CachedPoints[i] = transform.position;
                    //Debug.LogError("Caching: " + i + " wait: " + duration + " sec");
                    yield return new WaitForSeconds(duration);
                }
                photonView.RPC("RPC_OnReceivedTrajectoryUpdate", RpcTarget.Others, m_CachedPoints);
                Debug.LogError("RPC done");
            }
        }

        [PunRPC]
        private void RPC_OnReceivedTrajectoryUpdate(Vector3[] points)
        {
            //Debug.LogError("RPC_OnReceivedTrajectoryUpdate");
            m_Points = points;
            if (m_DrawEnumerator == null)
            {
                m_DrawEnumerator = DrawEnumerator();
                StartCoroutine(m_DrawEnumerator);
            }
            else
                m_ReceivedUpdate = true;
        }

        private IEnumerator DrawEnumerator()
        {
            var duration = (float)trajectoryUpdateRate / (m_Points.Length * 1000);
            Vector3[] points = m_Points;

            while (trailStatus == TrailStatus.DRAWING)
            {
                foreach (var item in points)
                {
                    //transform.DOMove(item, duration);
                    transform.position = item;
                    //Debug.LogError("updating position: wait: " + duration);
                    yield return new WaitForSeconds(duration);
                }
                yield return new WaitForTrajectoryUpdate(this);
                points = m_Points;
                m_ReceivedUpdate = false;
            }
        }

       
        #endregion
    }
}