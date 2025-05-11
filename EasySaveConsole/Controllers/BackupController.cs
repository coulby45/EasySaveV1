using System;
using System.Collections.Generic;
using EasySaveConsole.Managers;
using EasySaveConsole.Models;
using EasySaveConsole.Views;

namespace EasySaveConsole.Controllers
{
    public class BackupController
    {
        private readonly BackupManager _manager;
        private readonly BackupView _view;
        private string _language = "en";

        public BackupController()
        {
            _manager = new BackupManager();
            _view = new BackupView();
        }

        public void Start(string[] args)
        {
            if (args.Length > 0)
            {
                var indices = ParseArgs(args[0]);
                _manager.ExecuteJobsByIndices(indices);
                return;
            }

            _language = _view.ChooseLanguage();
            while (true)
            {
                var opt = _view.DisplayMenu(_language);
                if (opt == "6") break;
                HandleOption(opt);
            }
        }

        private void HandleOption(string option)
        {
            switch (option)
            {
                case "1":
                    var jb = _view.AskNewBackupInfo(_language);
                    if (!_manager.AddJob(jb))
                        _view.ShowMessage("max_jobs", _language);
                    break;
                case "2":
                    _view.DisplayBackupList(_manager.Jobs);
                    if (_view.ConfirmSearch(_language))
                    {
                        var name = _view.AskBackupName(_language);
                        var b = _manager.GetJob(name);
                        if (b != null) _view.DisplayBackup(b);
                        else _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "3":
                    {
                        var oldName = _view.AskBackupName(_language);
                        var nb = _view.AskNewBackupInfo(_language, oldName);
                        if (!_manager.UpdateJob(oldName, nb))
                            _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "4":
                    {
                        var name = _view.AskBackupName(_language);
                        if (!_manager.RemoveJob(name))
                            _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "5":
                    _manager.ShowLogs();
                    break;
                default:
                    _view.ShowMessage("invalid", _language);
                    break;
            }
        }

        private IEnumerable<int> ParseArgs(string arg)
        {
            var list = new List<int>();
            if (arg.Contains("-"))
            {
                var p = arg.Split('-');
                if (int.TryParse(p[0], out var a) && int.TryParse(p[1], out var b))
                    for (int i = a; i <= b; i++) list.Add(i);
            }
            else
            {
                foreach (var tok in arg.Split(';'))
                    if (int.TryParse(tok, out var x))
                        list.Add(x);
            }
            return list;
        }
    }
}
