using RDPStreaming.Viewer.ViewModel.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RDPStreaming.Viewer.ViewModel
{
    public class LoginDialogViewModel : BaseViewModel
    {
		private string _username;
		private string _password;
		private ICommand _submitCommand;
		private ICommand _cancelCommand;

		public ICommand CancelCommand
		{
			get 
			{
				if (_cancelCommand == null)
				{
					_cancelCommand = new RelayCommand(p => Cancel((Window)p), p => true);
				}
				return _cancelCommand; 
			}
		}

		public ICommand SubmitCommand
		{
			get 
			{
				if (_submitCommand == null)
				{
					_submitCommand = new RelayCommand(p => Submit((Window)p), p => true);
				}
				return _submitCommand; 
			}
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public string Username
		{
			get { return _username; }
			set { _username = value; OnPropertyChanged(nameof(Username)); }
		}

		private void Submit(Window view)
		{
			view.DialogResult = true;
			view.Close();
		}

		private void Cancel(Window view)
		{
			view.DialogResult = false;
			view.Close();
		}

	}
}
