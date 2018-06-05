using System;
using System.Reactive.Subjects;

namespace WorkspaceServer.WorkspaceFeatures
{
    public abstract class CommandLineObservable : IObservable<string>
    {
        private readonly ReplaySubject<string> _subject = new ReplaySubject<string>();

        internal void OnNext(string value) => _subject.OnNext(value);

        public IDisposable Subscribe(IObserver<string> observer) => _subject.Subscribe(observer);
    }
}