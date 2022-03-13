using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CanvasUpdater : MonoBehaviour
    {
        [SerializeField]
        private Text dungeonSizeText;
        [SerializeField]
        private Text minRoomHeightText;
        [SerializeField]
        private Text maxRoomHeightText;
        [SerializeField]
        private Text minRoomLengthText;
        [SerializeField]
        private Text maxRoomLengthText;
        [SerializeField]
        private Text roomsAmountText;
        [SerializeField]
        private Text minHallwayLengthText;
        [SerializeField]
        private Text maxHallwayLengthText;

        public int DungeonSize { get; private set; } = 100;
        public int MinRoomHeight { get; private set; } = 1;
        public int MaxRoomHeight { get; private set; } = 25;
        public int MinRoomLength { get; private set; } = 1;
        public int MaxRoomLength { get; private set; } = 25;
        public int RoomsAmount { get; private set; } = 5;
        public int MinHallwayLength { get; private set; } = 1;
        public int MaxHallwayLength { get; private set; } = 10;

        public void UpdateDungeonSize(float value)
        {
            DungeonSize = (int)value;
            dungeonSizeText.text = $"{value} x {value}";
        }

        public void UpdateMinRoomHeight(float value)
        {
            MinRoomHeight = (int)value;
            minRoomHeightText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxRoomHeight(float value)
        {
            MaxRoomHeight = (int)value;
            maxRoomHeightText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMinRoomLength(float value)
        {
            MinRoomLength = (int)value;
            minRoomLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxRoomLength(float value)
        {
            MaxRoomLength = (int)value;
            maxRoomLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateRoomsAmount(float value)
        {
            RoomsAmount = (int)value;
            roomsAmountText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMinHallwayLength(float value)
        {
            MinHallwayLength = (int)value;
            minHallwayLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxHallwayLength(float value)
        {
            MaxHallwayLength = (int) value;
            maxHallwayLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
