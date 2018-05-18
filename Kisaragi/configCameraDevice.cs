using System;

namespace configCamera
{
    class cameraDevice
    {
        public string deviceName;
        public int deviceID;
        public Guid identifier;

        public cameraDevice(int ID, string Name, Guid Identity = new Guid())
        {
            deviceID = ID;
            deviceName = Name;
            identifier = Identity;
        }

        public string ToStringL()
        {
            return String.Format("[{0}] {1}: {2}", deviceID, deviceName, identifier);
        }

        public string ToStringS()
        {
            return String.Format("[{0}] {1}", deviceID, deviceName);
        }
    }
}
