
using HFSM;

public class RootStateMachine : StateMachine {
	
	public RootStateMachine(params StateObject[] states) : base(UpdateMode.UpdateBeforeChild, states) { 
	
	}

	protected override void OnUpdate() { 
	
	}

	protected override void OnEnter() {
		
	}

	protected override void OnExit() {
	
	}
	public override object RecordFieldsAndProperties() {
		return null;
	}

	public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}