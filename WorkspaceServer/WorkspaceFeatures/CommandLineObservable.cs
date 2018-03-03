using System;
using System.Reactive.Subjects;

namespace WorkspaceServer.WorkspaceFeatures
{
    public abstract class CommandLineObservable : IObservable<string>
    {
        private readonly ReplaySubject<string> subject = new ReplaySubject<string>();

        internal void OnNext(string value) => subject.OnNext(value);

        public IDisposable Subscribe(IObserver<string> observer) => subject.Subscribe(observer);
    }
}