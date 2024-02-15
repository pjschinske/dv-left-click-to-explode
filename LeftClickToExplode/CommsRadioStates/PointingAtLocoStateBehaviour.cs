using CommsRadioAPI;
using DV.Damage;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeftClickToExplode.CommsRadioStates
{
	internal class PointingAtLocoStateBehaviour : PointingAtSomethingStateBehaviour
	{
		public PointingAtLocoStateBehaviour(TrainCar selectedCar) : base(selectedCar)
		{
		}

		public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
		{
			if (action != InputAction.Activate)
			{
				throw new ArgumentException();
			}
			//if we're a locomotive with an explosive resource, like fuel
			//(aka all diesel locomotives)
			ResourceExplosionBase resourceExplosionBase = selectedCar.GetComponent<ResourceExplosionBase>();
			if (resourceExplosionBase is not null)
			{
				resourceExplosionBase.ExplodeResource();
				return new PointingAtNothingStateBehaviour();
			}

			//if we're a locomotive with a boiler
			SimController simController = selectedCar.GetComponent<SimController>();
			if (simController is not null)
			{
				foreach (ASimInitializedController controller in simController.otherSimControllers)
				{

					if (controller is ExplosionActivationOnSignal)
					{
						//ExplosionActivationOnSignal exploder = (ExplosionActivationOnSignal)controller;
						//exploder.ExplodeTrainCar();

						//these three lines just do what exploder.ExplodeTrainCar() does but
						//without checking if the trainCar has been exploded yet.
						//This means we can explode a steam locomotive as much as we want,
						//which isn't very realistic but it is very fun
						TrainCarExplosion.CreateExplosion(10000000f, selectedCar.transform.position, 15f, -1f, 100f);
						TrainCarExplosion.UpdateModelToExploded(selectedCar);
						simController.resourceContainerController?.DepleteAllResourceContainers();

						return new PointingAtNothingStateBehaviour();
					}
				}
			}
			Main.Logger.Warning("I didn't think we'd get here. Let me know if you see this message");
			return new PointingAtNothingStateBehaviour();
		}
	}
}
