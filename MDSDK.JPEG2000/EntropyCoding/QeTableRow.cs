// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.EntropyCoding
{
    class QeTableRow
    {
        public int Index { get; }

        public ushort QeValue { get; }

        public int NextMPSIndex { get; }

        public int NextLPSIndex { get; }

        public bool Switch { get; }

        public QeTableRow(int index, ushort qeValue, int nextMPSIndex, int nextLPSIndex, bool @switch = false)
        {
            Index = index;
            QeValue = qeValue;
            NextMPSIndex = nextMPSIndex;
            NextLPSIndex = nextLPSIndex;
            Switch = @switch;
        }

        public static QeTableRow[] GetQeTableRows()
        {
            return new QeTableRow[47]
            {
                new QeTableRow(0, 0x5601, 1, 1, true),
                new QeTableRow(1, 0x3401, 2, 6),
                new QeTableRow(2, 0x1801, 3, 9),
                new QeTableRow(3, 0x0AC1, 4, 12),
                new QeTableRow(4, 0x0521, 5, 29),
                new QeTableRow(5, 0x0221, 38, 33),
                new QeTableRow(6, 0x5601, 7, 6, true),
                new QeTableRow(7, 0x5401, 8, 14),
                new QeTableRow(8, 0x4801, 9, 14),
                new QeTableRow(9, 0x3801, 10, 14),
                new QeTableRow(10, 0x3001, 11, 17),
                new QeTableRow(11, 0x2401, 12, 18),
                new QeTableRow(12, 0x1C01, 13, 20),
                new QeTableRow(13, 0x1601, 29, 21),
                new QeTableRow(14, 0x5601, 15, 14, true),
                new QeTableRow(15, 0x5401, 16, 14),
                new QeTableRow(16, 0x5101, 17, 15),
                new QeTableRow(17, 0x4801, 18, 16),
                new QeTableRow(18, 0x3801, 19, 17),
                new QeTableRow(19, 0x3401, 20, 18),
                new QeTableRow(20, 0x3001, 21, 19),
                new QeTableRow(21, 0x2801, 22, 19),
                new QeTableRow(22, 0x2401, 23, 20),
                new QeTableRow(23, 0x2201, 24, 21),
                new QeTableRow(24, 0x1C01, 25, 22),
                new QeTableRow(25, 0x1801, 26, 23),
                new QeTableRow(26, 0x1601, 27, 24),
                new QeTableRow(27, 0x1401, 28, 25),
                new QeTableRow(28, 0x1201, 29, 26),
                new QeTableRow(29, 0x1101, 30, 27),
                new QeTableRow(30, 0x0AC1, 31, 28),
                new QeTableRow(31, 0x09C1, 32, 29),
                new QeTableRow(32, 0x08A1, 33, 30),
                new QeTableRow(33, 0x0521, 34, 31),
                new QeTableRow(34, 0x0441, 35, 32),
                new QeTableRow(35, 0x02A1, 36, 33),
                new QeTableRow(36, 0x0221, 37, 34),
                new QeTableRow(37, 0x0141, 38, 35),
                new QeTableRow(38, 0x0111, 39, 36),
                new QeTableRow(39, 0x0085, 40, 37),
                new QeTableRow(40, 0x0049, 41, 38),
                new QeTableRow(41, 0x0025, 42, 39),
                new QeTableRow(42, 0x0015, 43, 40),
                new QeTableRow(43, 0x0009, 44, 41),
                new QeTableRow(44, 0x0005, 45, 42),
                new QeTableRow(45, 0x0001, 45, 43),
                new QeTableRow(46, 0x5601, 46, 46),
            };
        }
    }
}
