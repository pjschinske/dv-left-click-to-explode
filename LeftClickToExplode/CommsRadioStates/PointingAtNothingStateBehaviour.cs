using CommsRadioAPI;
using DV;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using DV.Damage;
using DV.ThingTypes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using JetBrains.Annotations;

namespace LeftClickToExplode.CommsRadioStates
{
	internal class PointingAtNothingStateBehaviour : AStateBehaviour
	{
		private const float SIGNAL_RANGE = 100f;

		private TrainCar selectedCar;
		private Transform signalOrigin;
		private int trainCarMask;

		public PointingAtNothingStateBehaviour()
			: base(new CommsRadioState(
				titleText: "Exploder",
				contentText: "Hover over something explodable",
				buttonBehaviour: ButtonBehaviourType.Regular))
		{
			
		}

		public override void OnEnter(CommsRadioUtility utility, AStateBehaviour? previous)
		{
			base.OnEnter(utility, previous);
			trainCarMask = LayerMask.GetMask(new string[]
			{
			"Train_Big_Collider"
			});

			//got to steal some components from other radio modes
			refreshSignalOriginAndTrainCarMask();
		}

		public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
		{
			Main.Logger.Warning("On Action was called while in PointingAtNothing");
			return new PointingAtNothingStateBehaviour();
		}

		private void refreshSignalOriginAndTrainCarMask()
		{
			trainCarMask = LayerMask.GetMask(new string[]
			{
			"Train_Big_Collider"
			});
			ICommsRadioMode? commsRadioMode = ControllerAPI.GetVanillaMode(VanillaMode.Clear);
			if (commsRadioMode is null)
			{
				Main.Logger.Error("Could not find CommsRadioCarDeleter");
				throw new NullReferenceException();
			}
			CommsRadioCarDeleter carDeleter = (CommsRadioCarDeleter)commsRadioMode;
			signalOrigin = carDeleter.signalOrigin;
		}

		//Highlighting of locomotives happens here
		public override AStateBehaviour OnUpdate(CommsRadioUtility utility)
		{
			while (signalOrigin is null)
			{
				Main.Logger.Warning("signalOrigin is null for some reason");
				refreshSignalOriginAndTrainCarMask();
			}

			RaycastHit hit;
			//if we're not pointing at anything
			if (!Physics.Raycast(signalOrigin.position, signalOrigin.forward, out hit, SIGNAL_RANGE, trainCarMask)) {
				return this;
			}

			//try to get the car we're pointing at
			TrainCar selectedCar = TrainCar.Resolve(hit.transform.root);

			//if we aren't pointing at a car
			if (selectedCar is null)
			{
				return this;
			}

			//if we're pointing at a locomotive
			SimController simController = selectedCar.GetComponent<SimController>();
			if (simController is not null)
			{
				foreach (ASimInitializedController controller in simController.otherSimControllers)
				{
					//if we're a locomotive that can explode
					//This should cover all locomotives in the game but we'll see
					if (controller is ExplosionActivationOnSignal
						|| controller is DeadTractionMotorsController)
					{
						return new PointingAtLocoStateBehaviour(selectedCar);
					} 
				}
			}
			else
			{
				//if this is a freight car
				CargoDamageModel cargoDamageModel = selectedCar.GetComponent<CargoDamageModel>();
				if (cargoDamageModel is not null)
				{
					CargoEffectsType cargoEffectsType =
						TrainCarAndCargoDamageProperties.CargoTypeToEffectsType(cargoDamageModel.cargoType);

					//if cargo exists and is explosive
					if (((int)cargoEffectsType & (int)CargoEffectsType.Explosive) > 0)
					{
						return new PointingAtExplosiveCargoStateBehaviour(selectedCar);
					}
				}
			}
			return this;
		}
	}
}
