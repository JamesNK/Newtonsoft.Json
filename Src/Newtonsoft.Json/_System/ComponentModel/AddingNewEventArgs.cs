using System;

namespace System.ComponentModel {
	
	public delegate void AddingNewEventHandler(object sender, AddingNewEventArgs e);
	
	public class AddingNewEventArgs {
		
		private object _newObject;
		
		public AddingNewEventArgs () {
		}
		
		public AddingNewEventArgs(object newObject) {
			this._newObject = newObject;
		}
		
		public virtual object NewObject {
			get {
				return this._newObject;
			}
		}
	}
}

