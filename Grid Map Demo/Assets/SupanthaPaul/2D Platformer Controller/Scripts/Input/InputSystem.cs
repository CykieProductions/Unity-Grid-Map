using UnityEngine;
using CykieProductions.Cytools;

namespace SupanthaPaul
{
	public class InputSystem : MonoBehaviour, IInputTracker
	{
		// input string caching
		static readonly string HorizontalInput = "Horizontal";
		static readonly string VerticalInput = "Vertical";
		static readonly string JumpInput = "Jump";
		static readonly string ActionInput = "Fire1";
		static readonly string DashInput = "Dash";
		static readonly string MapInput = "OpenMap";
		static readonly string SwitchInput = "Switch";
		static readonly string PauseInput = "Pause";

		void Awake()
		{
			IInputTracker.TrySetCurrent(this);
		}

		public static float HorizontalRaw()
		{
			return IInputTracker.Current.HorizontalRaw();
		}
		public static float VerticalRaw()
		{
			return IInputTracker.Current.VerticalRaw();
		}
		float IInputTracker.HorizontalRaw()
		{
			return Input.GetAxisRaw(HorizontalInput);
		}
		float IInputTracker.VerticalRaw()
		{
			return Input.GetAxisRaw(VerticalInput);
		}
		public static bool PressingSwitch()
		{
			return Input.GetButton(SwitchInput);
		}

		public static bool Jump()
		{
			return Input.GetButtonDown(JumpInput);
		}
		public static bool ActionDown()
		{
			return Input.GetButtonDown(ActionInput);
		}


		public static bool Dash()
		{
			return Input.GetButtonDown(DashInput);
		}
		public static bool PauseDown()
		{
			return Input.GetButtonDown(PauseInput);
		}
		public static bool OpenMap()
		{
			return Input.GetButtonDown(MapInput);
		}

	}
}
