using System;
using System.Collections.Generic;
using System.Windows;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities
{
    public delegate void DScanUpdate(IList<IDScanItem> items);

    public delegate void ScanAccessChanged(object newTarget);

    public interface IScanAccess
    {
        void RegisterCallback(object target, Action<IList<IDScanItem>> dscanCallback, Action<IList<IProbeScanItem>> probeScanCallback);
        void UnregisterCallback(object target);
        void GetExclusiveAccess(object target);
        void SetClipboardText(object access, string text);
        void RemoveAccess(object target);
        event ScanAccessChanged ScanAccessChanged;

    }

    public class ScanAccess : IScanAccess
    {
        private readonly IDictionary<object, Action<IList<IDScanItem>>> _callbacks = new Dictionary<object, Action<IList<IDScanItem>>>();
        private readonly IDictionary<object, string> _lastClipboardText = new Dictionary<object, string>();
        private readonly IDictionary<object, Action<IList<IProbeScanItem>>> _probeScanCallbacks = new Dictionary<object, Action<IList<IProbeScanItem>>>();
        private object _currentAccess;
        private Action<IList<IDScanItem>> _currentCallback;
        private Action<IList<IProbeScanItem>> _currentProbeScanCallback;

        public ScanAccess(ClipboardParser parser)
        {
            parser.DirectionalScan += ParserOnDirectionalScan;
            parser.ProbeScan += ParserOnProbeScan;
        }


        public event ScanAccessChanged ScanAccessChanged;

        protected virtual void OnScanAccessChanged(object newtarget)
        {
            var handler = ScanAccessChanged;
            if (handler != null) handler(newtarget);
        }

        private void ParserOnProbeScan(IList<IProbeScanItem> probeScanItems)
        {
            if (_currentProbeScanCallback == null)
            {
                return;
            }

            _currentProbeScanCallback(probeScanItems);
        }

        private void ParserOnDirectionalScan(IList<IDScanItem> dScanItems)
        {
            if (_currentCallback == null)
            {
                return;
            }

            _currentCallback(dScanItems);
        }

        public void RegisterCallback(object target, Action<IList<IDScanItem>> callback, Action<IList<IProbeScanItem>> probeScanCallback)
        {
            _callbacks.Add(target, callback);
            _probeScanCallbacks.Add(target, probeScanCallback);
        }

        public void UnregisterCallback(object target)
        {
            var callbackOfTarget = _callbacks[target];
            var probeScanCallbackOfTarget = _probeScanCallbacks[target];

            if (_currentCallback == callbackOfTarget)
            {
                _currentCallback = null;
            }

            if (_currentProbeScanCallback == probeScanCallbackOfTarget)
            {
                _currentProbeScanCallback = null;
            }
            _callbacks.Remove(target);
            _probeScanCallbacks.Remove(target);
            _lastClipboardText.Remove(target);
            if (_currentAccess == target)
            {
                _currentAccess = null;
                OnScanAccessChanged(null);
            }
        }

        public void SetClipboardText(object access, string text)
        {
            _lastClipboardText[access] = text;
            if (access == _currentAccess)
            {
                Clipboard.SetDataObject(text);
            }
        }

        public void RemoveAccess(object target)
        {
            if (_currentAccess != target)
            {
                return;
            }
            _currentAccess = null;
            _currentCallback = null;
            _currentProbeScanCallback = null;
            OnScanAccessChanged(null);
        }

        public void GetExclusiveAccess(object target)
        {
            _currentAccess = target;
            _currentCallback = _callbacks[target];
            _currentProbeScanCallback = _probeScanCallbacks[target];

            string lastClipboard;
            if (_lastClipboardText.TryGetValue(target, out lastClipboard))
            {
                Clipboard.SetDataObject(lastClipboard);
            }
            OnScanAccessChanged(target);
        }
    }
}
