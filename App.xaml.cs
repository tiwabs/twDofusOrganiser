using System;
using System.Threading;
using System.Windows;

namespace twDofusOrganiser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string MutexName = "TWDofusOrganiser_SingleInstance_v1";
        private Mutex? singleInstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            try
            {
                // Try to acquire the mutex with a timeout to detect dead instances
                singleInstanceMutex = new Mutex(true, MutexName, out createdNew);
                
                // If mutex was not created new, try to wait a bit to see if it's really in use
                if (!createdNew)
                {
                    singleInstanceMutex.Dispose();
                    singleInstanceMutex = null;
                    
                    // Try to open existing mutex and wait with timeout
                    Mutex? existingMutex = null;
                    try
                    {
                        existingMutex = Mutex.OpenExisting(MutexName);
                        // Wait up to 500ms to see if the existing instance releases the mutex
                        if (!existingMutex.WaitOne(500))
                        {
                            // Mutex is held by another instance, notify and exit
                            existingMutex.Dispose();
                            SingleInstance.PostShowMessage();
                            Shutdown();
                            return;
                        }
                        else
                        {
                            // We acquired the mutex, meaning the previous instance is dead
                            // Keep the mutex and continue
                            singleInstanceMutex = existingMutex;
                            createdNew = true;
                        }
                    }
                    catch (WaitHandleCannotBeOpenedException)
                    {
                        // Mutex doesn't exist anymore, create a new one
                        singleInstanceMutex = new Mutex(true, MutexName, out createdNew);
                    }
                    catch (AbandonedMutexException)
                    {
                        // Previous instance crashed and left the mutex abandoned
                        // We now own it, so continue
                        singleInstanceMutex = existingMutex;
                        createdNew = true;
                    }
                }
            }
            catch
            {
                // If anything goes wrong, just try to create a new mutex
                try
                {
                    singleInstanceMutex = new Mutex(true, MutexName, out createdNew);
                }
                catch
                {
                    // Last resort: start anyway
                    createdNew = true;
                }
            }

            if (!createdNew)
            {
                // Notify existing instance to show itself (or deactivate organizer and show)
                SingleInstance.PostShowMessage();

                // Exit this new instance
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                if (singleInstanceMutex != null)
                {
                    singleInstanceMutex.ReleaseMutex();
                    singleInstanceMutex.Dispose();
                    singleInstanceMutex = null;
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
