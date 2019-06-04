﻿// 
// TextPromptViewModel.cs
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
    public class TextPromptViewModel : ViewModelBase
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
                return Strings.TextPromptTitle;
            }
        }

        public string Prompt { get; private set; }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                RaisePropertyChanged(nameof(Text));
                Accept.RaiseCanExecuteChanged();
            }
        }
        private string _text;

        public bool AllowBlank
        {
            get
            {
                return _allowBlank;
            }
            set
            {
                _allowBlank = value;
                RaisePropertyChanged(nameof(AllowBlank));
                RaisePropertyChanged(nameof(Accept));
            }
        }
        private bool _allowBlank;

        public RelayCommand Accept
        {
            get
            {
                return _accept ?? (_accept = new RelayCommand(() =>
                {
                    try
                    {
                        RequestClose?.Invoke();
                        Callback(Text);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AllowBlank || !string.IsNullOrWhiteSpace(Text);
                }));
            }
        }
        private RelayCommand _accept;

        public RelayCommand Cancel
        {
            get
            {
                return _cancel ?? (_cancel = new RelayCommand(() =>
                {
                    try
                    {
                        RequestClose?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _cancel;

        public Action RequestClose;

        public Action<string> Callback { get; private set; }

        public TextPromptViewModel(string prompt, Action<string> callback)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            Prompt = prompt;
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }
    }
}
