using System.Text;


namespace Lab3
{
    public class NetworkSegment
    {
        private int _sourcePort;
        private int _destinationPort;
        private uint _seqNumber;
        private uint _ackNumber;
        private bool _isSYN;
        private bool _isACK;
        private bool _isRST;
        private bool _isFIN;
        private ushort _winSize;
        private byte[] _payload = [];

        public int SourcePort
        {
            get { return _sourcePort; }
            set { _sourcePort = value; }
        }

        public int DestinationPort
        {
            get { return _destinationPort; }
            set { _destinationPort = value; }
        }

        public uint SequenceNumber
        {
            get { return _seqNumber; }
            set { _seqNumber = value; }
        }

        public uint AcknowledgmentNumber
        {
            get { return _ackNumber; }
            set { _ackNumber = value; }
        }

        public bool SYNFlag
        {
            get { return _isSYN; }
            set { _isSYN = value; }
        }

        public bool ACKFlag
        {
            get { return _isACK; }
            set { _isACK = value; }
        }

        public bool RSTFlag
        {
            get { return _isRST; }
            set { _isRST = value; }
        }
            
        public bool FINFlag
        {
            get { return _isFIN; }
            set { _isFIN = value; }
        }

        public ushort WindowSize
        {
            get { return _winSize; }
            set { _winSize = value; }
        }

        public byte[] Data
        {
            get { return _payload; }
            set { _payload = value; }
        }

        public static NetworkSegment CreateEmptySegment(ushort window, int sourcePort, int destPort, uint seqNum, uint ackNum, bool ack = false, bool syn = false, bool rst = false, bool fin = false)
        {
            return new NetworkSegment
            {
                SourcePort = sourcePort,
                DestinationPort = destPort,
                SequenceNumber = seqNum,
                AcknowledgmentNumber = ackNum,
                SYNFlag = syn,
                ACKFlag = ack,
                RSTFlag = rst,
                FINFlag = fin,
                WindowSize = window,
            };
        }

        public static IEnumerable<NetworkSegment> GetPackets(byte[] data, ushort windowsSize, int sourcePort, int destinationPort, uint seqNum, uint ackNum)
        {
            List<NetworkSegment> packets = new();
            int id = 0;

            do
            {
                packets.Add(new NetworkSegment()
                {
                    SourcePort = sourcePort,
                    DestinationPort = destinationPort,
                    SequenceNumber = seqNum,
                    AcknowledgmentNumber = ackNum,
                    WindowSize = windowsSize,
                    Data = data[id..(id + windowsSize < data.Length ? id + windowsSize : data.Length)],
                });

                seqNum += windowsSize;
            }
            while ((id += windowsSize) < data.Length);

            return packets;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\n" +
                "================\n" +
                "Ports: [SrcPort: {0}, DestPort: {1}]" +
                "\nSeqNum: {2}, AckNum: {3}\n" +
                "Flags: [SYN: {4}, ACK: {5}, RST: {6}, FIN:{7}]\nWinSize: {8}\nDataLength: {9}\n" +
                "================",
                SourcePort, DestinationPort, SequenceNumber, AcknowledgmentNumber, SYNFlag, ACKFlag, RSTFlag, FINFlag, WindowSize, Data.Length);
            return sb.ToString();
        }
    }
}