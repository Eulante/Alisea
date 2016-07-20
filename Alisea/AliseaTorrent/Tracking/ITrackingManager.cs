using System.Collections.Generic;

namespace AliseaTorrent.Tracking
{

    public interface ITrackingManager
    {

        /// <summary>
        /// Method which iterates for each tracker in the list, making a request to their related server
        /// in order to obtain the list of the peers with the timer needed to schedule the different requests.
        /// </summary>
        void StartTrackingRoutine();

        /// <summary>
        /// Method which iterates for each tracker in the list, making a request to their related server
        /// in order to obtain the list of the peers with the timer needed to schedule the different requests.
        /// </summary>
        void StopTrackingRoutine(AbstractTracker a);

        /// <summary>
        /// Set tracking list and timers list to null and tells every tracker server about
        /// its intention to stop the communication with them.
        /// </summary>
        void Reset();

        /// <summary>
        ///  Unusued because the tracker list is given inside the constructor.
        /// </summary>
        /// <param name="trackers"></param>
        void SetTrackers(List<AbstractTracker> trackers);

        /// <summary>
        /// Sets the tracking listener to the relative class field.
        /// </summary>
        /// <param name="resultListener">The rvalue object</param>
        /// 
        void SetResultListener(ITrackingResultListener resultListener);

    }
}
