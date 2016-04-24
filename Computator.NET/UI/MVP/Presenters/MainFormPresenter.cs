﻿#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;
using Computator.NET.Localization;
using Computator.NET.Properties;
using Computator.NET.UI.Commands;
using Computator.NET.UI.MVP;
using Computator.NET.UI.MVP.Views;

namespace Computator.NET
{
    public class MainFormPresenter
    {
        private readonly CultureInfo[] _allCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);


        private readonly IMainForm _view;


        private CalculationsMode _calculationsMode;

        public MainFormPresenter(IMainForm view)
        {




            _view = view;

            _view.ToolbarView.SetCommands(new List<IToolbarCommand>()
            {
                new NewCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor),
                new OpenCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor),
                new SaveCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor),
                new PrintCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor, _view),
                null,

                new CutCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor, _view),
                                new CopyCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor, _view),
                                                new PasteCommand(_view.ScriptingView.CodeEditorView,_view.CustomFunctionsView.CustomFunctionsEditor, _view),
               null,
                         new HelpCommand(),
                               null,
              new ExponentCommand(),
               null,
               new RunCommand()
            });


            _view.MenuStripView.SetCommands(MenuStripCommandBuilder.BuildMenuStripCommands(this._view));

          //  _view.EnterClicked += (o, e) => SharedViewState.Instance.CurrentAction?.Invoke(o, e);

            var expressionViewPresenter = new ExpressionViewPresenter(_view.ExpressionView);




            _view.ModeForcedToReal += (sender, args) =>
            {
                //   SetMode(CalculationsMode.Real);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            };
            _view.ModeForcedToComplex += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Complex);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
            };
            _view.ModeForcedToFxy += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Fxy);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Fxy));
            };

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(mode => SetMode(mode.CalculationsMode));

            _view.SetLanguages(new object[]
            {
                new CultureInfo("en").NativeName,
                new CultureInfo("pl").NativeName,
                new CultureInfo("de").NativeName,
                new CultureInfo("cs").NativeName
            });
            _view.SelectedLanguageChanged += _view_SelectedLanguageChanged;


            _view.SelectedLanguage =
                _allCultures.First(c =>
                    c.TwoLetterISOLanguageName ==
                    Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    .NativeName;

            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.Default.Language):
                        
                        Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
                        LocalizationManager.GlobalUICulture = Settings.Default.Language;
                        //////////////////////////////////_view.Restart();
                        break;
                }
            };

            ///////EventAggregator.Instance.Subscribe<ChangeViewEvent>(cv => { _view.SelectedViewIndex = (int) cv.View; });

            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SharedViewState.Instance.CurrentView):
                        _view.SelectedViewIndex = (int)SharedViewState.Instance.CurrentView;
                        break;
                }
            };

            _view.SelectedViewChanged += _view_SelectedViewChanged;

            _view.StatusText = GlobalConfig.version;
        }

        private void _view_SelectedViewChanged(object sender, EventArgs e)
        {
            SharedViewState.Instance.CurrentView = (ViewName) _view.SelectedViewIndex;
        }

        private void _view_SelectedLanguageChanged(object sender, EventArgs e)
        {
            var selectedCulture = _allCultures.First(c => c.NativeName == _view.SelectedLanguage);
            Thread.CurrentThread.CurrentCulture = selectedCulture;
            LocalizationManager.GlobalUICulture = selectedCulture;
            Settings.Default.Language = selectedCulture;
            Settings.Default.Save();
        }



        private void SetMode(CalculationsMode mode)
        {
            if (mode == _calculationsMode)
                return;


            switch (mode)
            {
                case CalculationsMode.Complex:
                    _view.ModeText = "Mode[Complex : f(z)]";
                    break;
                case CalculationsMode.Fxy:
                    _view.ModeText = "Mode[3D : f(x,y)]";
                    break;
                case CalculationsMode.Real:
                    _view.ModeText = "Mode[Real : f(x)]";
                    break;
            }

            _calculationsMode = mode;
            //  _view.EditChartMenus.SetMode(_calculationsMode);
        }
    }
}