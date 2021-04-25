using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CanvasUpdater : MonoBehaviour
    {
        public Text DungeonSizeText;
        public Text MinRoomWidthText;
        public Text MaxRoomWidthText;
        public Text MinRoomLengthText;
        public Text MaxRoomLengthText;
        public Text RoomsAmountText;
        public Text MinHallwayLengthText;
        public Text MaxHallwayLengthText;

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
            DungeonSizeText.text = $"{value} x {value}";
        }

        public void UpdateMinRoomHeight(float value)
        {
            MinRoomHeight = (int)value;
            MinRoomWidthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxRoomHeight(float value)
        {
            MaxRoomHeight = (int)value;
            MaxRoomWidthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMinRoomLength(float value)
        {
            MinRoomLength = (int)value;
            MinRoomLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxRoomLength(float value)
        {
            MaxRoomLength = (int)value;
            MaxRoomLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateRoomsAmount(float value)
        {
            RoomsAmount = (int)value;
            RoomsAmountText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMinHallwayLength(float value)
        {
            MinHallwayLength = (int)value;
            MinHallwayLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void UpdateMaxHallwayLength(float value)
        {
            MaxHallwayLength = (int) value;
            MaxHallwayLengthText.text = value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
