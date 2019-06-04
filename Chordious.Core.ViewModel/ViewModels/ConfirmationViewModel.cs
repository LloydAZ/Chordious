﻿// 
// ConfirmationViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2019 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Chordious.Core.ViewModel.Resources;

namespace Chordious.Core.ViewModel
{
    public class ConfirmationViewModel : ViewModelBase
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public string Title
        {
            get
            {
                return Strings.ConfirmationTitle;
            }
        }

        public string Message { get; private set; }

        public bool DisplayDialog { get; private set; } = true;

        public string YesAndRememberLabel
        {
            get
            {
                return Strings.YesAndRememberLabel;
            }
        }

        public bool ShowAcceptAndRemember
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_rememberAnswerKey);
            }
        }
        private readonly string _rememberAnswerKey;

        public RelayCommand AcceptAndRemember
        {
            get
            {
                return _acceptAndRemember ?? (_acceptAndRemember = new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.AcceptAndRemember;
                        RequestClose?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return ShowAcceptAndRemember;
                }));
            }
        }
        private RelayCommand _acceptAndRemember;

        public RelayCommand Accept
        {
            get
            {
                return _accept ?? (_accept = new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.Accept;
                        RequestClose?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _accept;

        public RelayCommand Reject
        {
            get
            {
                return _reject ?? (_reject = new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.Reject;
                        RequestClose?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _reject;

        public ConfirmationResult Result { get; private set; }

        public Action RequestClose;

        public Action<bool> Callback { get; private set; }

        public ConfirmationViewModel(string message, Action<bool> callback, string rememberAnswerKey)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Result = ConfirmationResult.None;

            _rememberAnswerKey = rememberAnswerKey;

            if (ShowAcceptAndRemember)
            {
                if (AppVM.Settings.TryGet(rememberAnswerKey, out bool value))
                {
                    if (value)
                    {
                        DisplayDialog = false;
                        Result = ConfirmationResult.AcceptAndRemember;
                    }
                }
            }
        }

        public void ProcessClose()
        {
            if (ShowAcceptAndRemember)
            {
                AppVM.Settings.Set(_rememberAnswerKey, Result == ConfirmationResult.AcceptAndRemember);
            }
            
            Callback(Result == ConfirmationResult.Accept || Result == ConfirmationResult.AcceptAndRemember);
        }
    }

    public enum ConfirmationResult
    {
        None,
        Accept,
        AcceptAndRemember,
        Reject
    }
}
