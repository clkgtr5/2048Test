﻿using UnityEngine;
using FairyGUI;
using DG.Tweening;
using System.Collections.Generic;

public class GestureMain : MonoBehaviour
{
	GComponent _mainView;
	Transform _ball;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/Gesture");
	}

	void Start()
	{
		_mainView = this.GetComponent<UIPanel>().ui;
        
        GObject holder = _mainView.GetChild("holder");

        _ball = GameObject.Find("Globe").transform;

		SwipeGesture gesture1 = new SwipeGesture(holder);
		gesture1.onMove.Add(OnSwipeMove);
		gesture1.onEnd.Add(OnSwipeEnd);

		LongPressGesture gesture2 = new LongPressGesture(holder);
		gesture2.once = false;
		gesture2.onAction.Add(OnHold);

		PinchGesture gesture3 = new PinchGesture(holder);
		gesture3.onAction.Add(OnPinch);

		RotationGesture gesture4 = new RotationGesture(holder);
		gesture4.onAction.Add(OnRotate);
	}

	void OnSwipeMove(EventContext context)
	{
		SwipeGesture gesture = (SwipeGesture)context.sender;
		Vector3 v = new Vector3();
		if (Mathf.Abs(gesture.delta.x) > Mathf.Abs(gesture.delta.y)) //delta 是触摸的距离
		{
			v.y = -Mathf.Round(gesture.delta.x);
			if (Mathf.Abs(v.y) < 2) //消除手抖的影响
				return;
		}
		else
		{
			v.x = -Mathf.Round(gesture.delta.y);
			if (Mathf.Abs(v.x) < 2)
				return;
		}
		_ball.Rotate(v, Space.World);
	}

	void OnSwipeEnd(EventContext context)
	{
		SwipeGesture gesture = (SwipeGesture)context.sender;
		Vector3 v = new Vector3();
		if (Mathf.Abs(gesture.velocity.x) > Mathf.Abs(gesture.velocity.y))
		{
			v.y = -Mathf.Round(Mathf.Sign(gesture.velocity.x) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.x)));
			if (Mathf.Abs(v.y) < 2)
				return;
		}
		else
		{
			v.x = -Mathf.Round(Mathf.Sign(gesture.velocity.y) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.y)));
			if (Mathf.Abs(v.x) < 2)
				return;
		}
		_ball.DORotate(v, 0.3f, RotateMode.WorldAxisAdd);
	}

	void OnHold(EventContext context)
	{
		_ball.DOShakePosition(0.3f, new Vector3(0.1f, 0.1f, 0));
	}

	void OnPinch(EventContext context)
	{
		DOTween.Kill(_ball);

		PinchGesture gesture = (PinchGesture)context.sender;
		float newValue = Mathf.Clamp(_ball.localScale.x + gesture.delta, 0.3f, 2);
		_ball.localScale = new Vector3(newValue, newValue, newValue);
	}

	void OnRotate(EventContext context)
	{
		DOTween.Kill(_ball);

		RotationGesture gesture = (RotationGesture)context.sender;
		_ball.Rotate(Vector3.forward, -gesture.delta, Space.World);
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}