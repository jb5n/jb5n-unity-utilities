using System;
using System.Collections.Generic;

// Use this for managing locks on actions, ensuring that certain actions are only performed when no locks are present.
// This could be used e.g. for preventing player input if any of a variety of states are present, such as being dead or in a menu.
public class ActionLock {
	public delegate void LockChangedHandler();
	public event LockChangedHandler OnLockAdded;
	public event LockChangedHandler OnLockRemoved;
	public event LockChangedHandler OnUnlock;

	protected List<Guid> locks = new();

	public Guid AddLock() {
		Guid newLock = Guid.NewGuid();
		locks.Add(newLock);
		OnLockAdded?.Invoke();
		return newLock;
	}

	public void ReleaseLock(Guid lockId) {
		if (locks.Count == 0) {
			return;
		}
		locks.Remove(lockId);
		OnLockRemoved?.Invoke();
		if (locks.Count == 0) {
			OnUnlock?.Invoke();
		}
	}

	public bool IsLocked() {
		return locks.Count > 0;
	}
}