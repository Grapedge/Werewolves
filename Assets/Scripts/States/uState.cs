using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有状态的基类
/// </summary>
public class uState<uType> {

	/// <summary>
	/// 目标物件，目标物件应该具有 SetNextState 的类似函数对其进行更新
	/// </summary>
	public uType target;

	public virtual void Enter (uType target) {
		this.target = target;
	}

	public virtual void Excute () { }

	public virtual void Exit () { }

}