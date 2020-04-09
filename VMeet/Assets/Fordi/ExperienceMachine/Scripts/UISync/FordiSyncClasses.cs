using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.Sync
{
    public interface IFordiObservable
    {
        int ViewId { get; }
        Selectable Selectable { get; }
        void OnValueChanged<T>(int viewId, T val);
        void Select(int viewId);
        void PointerClickEvent(int viewId);
        /// <summary>
        /// Called by PUN several times per second, so that your script can write and read synchronization data for the PhotonView.
        /// </summary>
        /// <remarks>
        /// This method will be called in scripts that are assigned as Observed component of a PhotonView.<br/>
        /// PhotonNetwork.SerializationRate affects how often this method is called.<br/>
        /// PhotonNetwork.SendRate affects how often packages are sent by this client.<br/>
        ///
        /// Implementing this method, you can customize which data a PhotonView regularly synchronizes.
        /// Your code defines what is being sent (content) and how your data is used by receiving clients.
        ///
        /// Unlike other callbacks, <i>OnPhotonSerializeView only gets called when it is assigned
        /// to a PhotonView</i> as PhotonView.observed script.
        ///
        /// To make use of this method, the PhotonStream is essential. It will be in "writing" mode" on the
        /// client that controls a PhotonView (PhotonStream.IsWriting == true) and in "reading mode" on the
        /// remote clients that just receive that the controlling client sends.
        ///
        /// If you skip writing any value into the stream, PUN will skip the update. Used carefully, this can
        /// conserve bandwidth and messages (which have a limit per room/second).
        ///
        /// Note that OnPhotonSerializeView is not called on remote clients when the sender does not send
        /// any update. This can't be used as "x-times per second Update()".
        /// </remarks>
        /// \ingroup publicApi
        void OnFordiSerializeView(FordiStream stream, FordiMessageInfo info);
    }

    /// <summary>
    /// This container is used in OnPhotonSerializeView() to either provide incoming data of a PhotonView or for you to provide it.
    /// </summary>
    /// <remarks>
    /// The IsWriting property will be true if this client is the "owner" of the PhotonView (and thus the GameObject).
    /// Add data to the stream and it's sent via the server to the other players in a room.
    /// On the receiving side, IsWriting is false and the data should be read.
    ///
    /// Send as few data as possible to keep connection quality up. An empty PhotonStream will not be sent.
    ///
    /// Use either Serialize() for reading and writing or SendNext() and ReceiveNext(). The latter two are just explicit read and
    /// write methods but do about the same work as Serialize(). It's a matter of preference which methods you use.
    /// </remarks>
    /// \ingroup publicApi
    public class FordiStream
    {
        private List<object> writeData;
        private object[] readData;
        private int currentItem; //Used to track the next item to receive.

        /// <summary>If true, this client should add data to the stream to send it.</summary>
        public bool IsWriting { get; private set; }

        /// <summary>If true, this client should read data send by another client.</summary>
        public bool IsReading
        {
            get { return !this.IsWriting; }
        }

        /// <summary>Count of items in the stream.</summary>
        public int Count
        {
            get { return this.IsWriting ? this.writeData.Count : this.readData.Length; }
        }

        /// <summary>
        /// Creates a stream and initializes it. Used by PUN internally.
        /// </summary>
        public FordiStream(bool write, object[] incomingData)
        {
            this.IsWriting = write;

            if (!write && incomingData != null)
            {
                this.readData = incomingData;
            }
        }

        public void SetReadStream(object[] incomingData, int pos = 0)
        {
            this.readData = incomingData;
            this.currentItem = pos;
            this.IsWriting = false;
        }

        internal void SetWriteStream(List<object> newWriteData, int pos = 0)
        {
            if (pos != newWriteData.Count)
            {
                throw new Exception("SetWriteStream failed, because count does not match position value. pos: " + pos + " newWriteData.Count:" + newWriteData.Count);
            }
            this.writeData = newWriteData;
            this.currentItem = pos;
            this.IsWriting = true;
        }

        internal List<object> GetWriteStream()
        {
            return this.writeData;
        }


        [Obsolete("Either SET the writeData with an empty List or use Clear().")]
        internal void ResetWriteStream()
        {
            this.writeData.Clear();
        }

        /// <summary>Read next piece of data from the stream when IsReading is true.</summary>
        public object ReceiveNext()
        {
            if (this.IsWriting)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
                return null;
            }

            object obj = this.readData[this.currentItem];
            this.currentItem++;
            return obj;
        }

        /// <summary>Read next piece of data from the stream without advancing the "current" item.</summary>
        public object PeekNext()
        {
            if (this.IsWriting)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
                return null;
            }

            object obj = this.readData[this.currentItem];
            //this.currentItem++;
            return obj;
        }

        /// <summary>Add another piece of data to send it when IsWriting is true.</summary>
        public void SendNext(object obj)
        {
            if (!this.IsWriting)
            {
                Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
                return;
            }

            this.writeData.Add(obj);
        }

        [Obsolete("writeData is a list now. Use and re-use it directly.")]
        public bool CopyToListAndClear(List<object> target)
        {
            if (!this.IsWriting) return false;

            target.AddRange(this.writeData);
            this.writeData.Clear();

            return true;
        }

        /// <summary>Turns the stream into a new object[].</summary>
        public object[] ToArray()
        {
            return this.IsWriting ? this.writeData.ToArray() : this.readData;
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref bool myBool)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(myBool);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    myBool = (bool)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref int myInt)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(myInt);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    myInt = (int)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref string value)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(value);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    value = (string)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref char value)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(value);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    value = (char)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref short value)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(value);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    value = (short)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref float obj)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(obj);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    obj = (float)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }


        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref Vector3 obj)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(obj);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    obj = (Vector3)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref Vector2 obj)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(obj);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    obj = (Vector2)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's IsWriting value.
        /// </summary>
        public void Serialize(ref Quaternion obj)
        {
            if (this.IsWriting)
            {
                this.writeData.Add(obj);
            }
            else
            {
                if (this.readData.Length > this.currentItem)
                {
                    obj = (Quaternion)this.readData[this.currentItem];
                    this.currentItem++;
                }
            }
        }
    }

    /// <summary>
    /// Container class for info about a particular message, RPC or update.
    /// </summary>
    /// \ingroup publicApi
    public struct FordiMessageInfo
    {
        private readonly int timeInt;
        /// <summary>The sender of a message / event. May be null.</summary>
        public readonly SyncView photonView;

        public FordiMessageInfo(int timestamp, SyncView view)
        {
            this.timeInt = timestamp;
            this.photonView = view;
        }

        [Obsolete("Use SentServerTime instead.")]
        public double timestamp
        {
            get
            {
                uint u = (uint)this.timeInt;
                double t = u;
                return t / 1000.0d;
            }
        }

        public double SentTime
        {
            get
            {
                uint u = (uint)this.timeInt;
                double t = u;
                return t / 1000.0d;
            }
        }

        public int SentTimeStamp
        {
            get { return this.timeInt; }
        }

        public override string ToString()
        {
            return string.Format("[FordiMessageInfo: Senttime={0}]", this.SentTime);
        }
    }

    public class SyncViewPair
    {
        private SyncView m_first;
        private SyncView m_second;

        public SyncView First { get => m_first; set => m_first = value; }
        public SyncView Second { get => m_second; set => m_second = value; }

        public void Register(SyncView view)
        {

            if (view == null)
            {
                Debug.LogError("view null");
                return;
            }

            if (m_first == null)
            {
                m_first = view;
                return;
            }

            if (m_second == null)
            {
                m_second = view;
            }
        }

        public SyncView GetPair(SyncView view)
        {
            //Debug.LogError(First.name);
            //Debug.LogError(view.name);
            //Debug.LogError(Second.name);
            if (view == null)
                return null;
            if (view == First)
                return Second;
            if (view == Second)
                return First;
            return null;
        }
    }
}
