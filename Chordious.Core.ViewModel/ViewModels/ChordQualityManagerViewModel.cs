﻿// 
// ChordQualityManagerViewModel.cs
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

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Chordious.Core.ViewModel.Resources;

namespace Chordious.Core.ViewModel
{
    public class ChordQualityManagerViewModel : NamedIntervalManagerViewModel
    {
        public override string Title
        {
            get
            {
                return Strings.ChordQualityManagerTitle;
            }
        }

        public override string DefaultNamedIntervalGroupLabel
        {
            get
            {
                return Strings.ChordQualityManagerDefaultNamedIntervalGroupLabel;
            }
        }

        public override string DefaultNamedIntervalGroupToolTip
        {
            get
            {
                return Strings.ChordQualityManagerDefaultNamedIntervalGroupToolTip;
            }
        }

        public override string UserNamedIntervalGroupLabel
        {
            get
            {
                return Strings.ChordQualityManagerUserNamedIntervalGroupLabel;
            }
        }

        public override string UserNamedIntervalGroupToolTip
        {
            get
            {
                return Strings.ChordQualityManagerUserNamedIntervalGroupToolTip;
            }
        }

        public override string AddNamedIntervalToolTip
        {
            get
            {
                return Strings.ChordQualityManagerAddNamedIntervalToolTip;
            }
        }

        public override RelayCommand AddNamedInterval
        {
            get
            {
                return _addNamedInterval ?? (_addNamedInterval = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ShowChordQualityEditorMessage((name, abbreviation, intervals) =>
                        {
                            try
                            {
                                ChordQuality addedChordQuality = AppVM.UserConfig.ChordQualities.Add(name, abbreviation, intervals);
                                Refresh(addedChordQuality);
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _addNamedInterval;

        public override string EditNamedIntervalToolTip
        {
            get
            {
                return Strings.ChordQualityManagerEditNamedIntervalToolTip;
            }
        }

        public override RelayCommand EditNamedInterval
        {
            get
            {
                return _editNamedInterval ?? (_editNamedInterval = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ShowChordQualityEditorMessage(SelectedNamedInterval.Name, ((ChordQuality)(SelectedNamedInterval.NamedInterval)).Abbreviation, SelectedNamedInterval.Intervals, SelectedNamedInterval.ReadOnly, (name, abbreviation, intervals) =>
                        {
                            try
                            {
                                ChordQuality cq = (ChordQuality)(SelectedNamedInterval.NamedInterval);
                                cq.Update(name, abbreviation, intervals);
                                Refresh(SelectedNamedInterval.NamedInterval);
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return NamedIntervalIsSelected;
                }));
            }
        }
        private RelayCommand _editNamedInterval;

        public override string DeleteNamedIntervalToolTip
        {
            get
            {
                return Strings.ChordQualityManagerDeleteNamedIntervalToolTip;
            }
        }

        public override RelayCommand DeleteNamedInterval
        {
            get
            {
                return _deleteNamedInterval ?? (_deleteNamedInterval = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ConfirmationMessage(string.Format(Strings.ChordQualityManagerDeleteNamedIntervalPromptFormat, SelectedNamedInterval.LongName), (confirm) =>
                        {
                            try
                            {
                                if (confirm)
                                {
                                    _deleteUserNamedInterval(SelectedNamedInterval.NamedInterval);
                                    Refresh();
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }, "confirmation.chordqualitymanager.deletenamedinterval"));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return NamedIntervalIsSelected && !SelectedNamedInterval.ReadOnly;
                }));
            }
        }
        private RelayCommand _deleteNamedInterval;

        public ChordQualityManagerViewModel() : base(AppViewModel.Instance.GetDefaultChordQualities, AppViewModel.Instance.GetUserChordQualities, AppViewModel.Instance.UserConfig.ChordQualities.Remove) { }
    }
}
