using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FSM 状态机驱动
/// </summary>
public class StateManager<uType> {

	protected uState<uType> m_GobalState = null;	// 全局状态

	protected uState<uType> m_PreState = null;		// 上一状态
	protected uState<uType> m_CurState = null;		// 当前状态

	protected uType m_Owner;

	public StateManager (uType owner) {
		m_Owner = owner;
	}

	//========================================属性
	public uState<uType> gobalState {
		get { return m_GobalState; }
		set { 
			if (m_GobalState != null) { 
				m_GobalState.Exit ();
			} 
			m_GobalState = value;
			if (m_GobalState != null) {
				m_GobalState.Enter (m_Owner); 
			} 
		}
	}

	public uState<uType> curState { 
		get { return m_CurState; } 
		set { 
			m_PreState = m_CurState; 
			if (m_CurState != null) {
				m_CurState.Exit ();
			}
			m_CurState = value;
			if (m_CurState != null) m_CurState.Enter (m_Owner);
		}
	}

	public uState<uType> preState { get { return m_PreState; } }

	public void Update () {
		if (m_GobalState != null) {
			m_GobalState.Excute ();
		}
		if (m_CurState != null) {
			m_CurState.Excute ();
		}
	}

	public void ChangeState (uState<uType> nextState) {
		curState = nextState;
	}
}
