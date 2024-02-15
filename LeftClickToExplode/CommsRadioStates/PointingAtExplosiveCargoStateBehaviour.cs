using CommsRadioAPI;
using DV.Damage;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeftClickToExplode.CommsRadioStates
{
	internal class PointingAtExplosiveCargoStateBehaviour : PointingAtSomethingStateBehaviour
	{
		public PointingAtExplosiveCargoStateBehaviour(TrainCar selectedCar) : base(selectedCar)
		{
		}

		public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
		{
			if (action != InputAction.Activate)
			{
				throw new ArgumentException();
			}
			CargoDamageModel cargoDamageModel = selectedCar.CargoDamage;
			if (cargoDamageModel == null)
			{
				Main.Logger.Error("CargoDamageModel null");
				throw new NullReferenceException();
			}
			cargoDamageModel.DestroyCargo();

			return new PointingAtNothingStateBehaviour();
		}
	}
}
