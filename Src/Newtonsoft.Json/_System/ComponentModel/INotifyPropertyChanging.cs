using System;

namespace System.ComponentModel {
	
	public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);
	
	public interface INotifyPropertyChanging {
		
		event PropertyChangingEventHandler PropertyChanging;
	}
}

