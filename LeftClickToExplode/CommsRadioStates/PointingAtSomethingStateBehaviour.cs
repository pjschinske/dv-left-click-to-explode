using CommsRadioAPI;
using DV;
using DV.Logic.Job;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using UnityEngine;

namespace LeftClickToExplode.CommsRadioStates
{
	internal abstract class PointingAtSomethingStateBehaviour : AStateBehaviour
	{
		private const float SIGNAL_RANGE = 200f;
		private static readonly Vector3 HIGHLIGHT_BOUNDS_EXTENSION = new Vector3(0.25f, 0.8f, 0f);

		internal TrainCar selectedCar;
		private Transform signalOrigin;
		private int trainCarMask;

		private GameObject highlighter;

		public PointingAtSomethingStateBehaviour(TrainCar selectedCar)
			: base(new CommsRadioState(
				titleText: "Exploder",
				contentText: "Explode this car",
				buttonBehaviour: ButtonBehaviourType.Regular))
		{
			this.selectedCar = selectedCar;
			if (this.selectedCar is null)
			{
				Main.Logger.Error("Null selectedCar");
				throw new ArgumentNullException(nameof(selectedCar));
			}

			//got to steal some components from other radio modes
			ICommsRadioMode? commsRadioMode = ControllerAPI.GetVanillaMode(VanillaMode.Clear);
			if (commsRadioMode is null)
			{
				Main.Logger.Error("Could not find CommsRadioCarDeleter");
				throw new NullReferenceException();
			}
			CommsRadioCarDeleter carDeleter = (CommsRadioCarDeleter)commsRadioMode;
			signalOrigin = carDeleter.signalOrigin;
			highlighter = carDeleter.trainHighlighter;
			highlighter.SetActive(false);
			highlighter.transform.SetParent(null);
		}

		public void Awake()
		{
			
		}

		public override AStateBehaviour OnUpdate(CommsRadioUtility utility)
		{
			RaycastHit hit;
			//if we're not pointing at anything
			if (!Physics.Raycast(signalOrigin.position, signalOrigin.forward, out hit, SIGNAL_RANGE, trainCarMask)) {
				return new PointingAtNothingStateBehaviour();
			}
			TrainCar target = TrainCar.Resolve(hit.transform.root);
			if (target is null || target != selectedCar)
			{
				//if we stopped pointing at selectedCar and are now pointing at either
				//nothing or another train car, then go back to PointingAtNothing so
				//we can figure out what we're pointing at
				return new PointingAtNothingStateBehaviour();
			}
			return this;
		}

		public override void OnEnter(CommsRadioUtility utility, AStateBehaviour? previous)
		{
			base.OnEnter(utility, previous);
			trainCarMask = LayerMask.GetMask(new string[]
			{
			"Train_Big_Collider"
			});

			MeshRenderer highlighterRenderer = highlighter.GetComponentInChildren<MeshRenderer>(true);
			highlighterRenderer.material = utility.GetMaterial(VanillaMaterial.Valid);

			highlighter.transform.localScale = selectedCar.Bounds.size + HIGHLIGHT_BOUNDS_EXTENSION;
			Vector3 b = selectedCar.transform.up * (highlighter.transform.localScale.y / 2f);
			Vector3 b2 = selectedCar.transform.forward * selectedCar.Bounds.center.z;
			Vector3 position = selectedCar.transform.position + b + b2;

			highlighter.transform.SetPositionAndRotation(position, selectedCar.transform.rotation);
			highlighter.SetActive(true);
			highlighter.transform.SetParent(selectedCar.transform, true);
		}

		public override void OnLeave(CommsRadioUtility utility, AStateBehaviour? next)
		{
			base.OnLeave(utility, next);
			highlighter.SetActive(false);
			highlighter.transform.SetParent(null);
		}
	}
}
