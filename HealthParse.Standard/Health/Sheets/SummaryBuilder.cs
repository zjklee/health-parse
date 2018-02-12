﻿using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : ISheetBuilder
    {
        private readonly IReadOnlyDictionary<string, IEnumerable<Record>> _records;
        private readonly IReadOnlyDictionary<string, IEnumerable<Workout>> _workouts;
        private readonly DateTimeZone _zone;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _hiitBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;
        private readonly ISheetBuilder<MassBuilder.MassItem> _massBuilder;
        private readonly ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem> _bodyFatBuilder;

        public SummaryBuilder(IReadOnlyDictionary<string, IEnumerable<Record>> records,
            IReadOnlyDictionary<string, IEnumerable<Workout>> workouts,
            DateTimeZone zone,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> runningBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> strengthBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> hiitBuilder,
            ISheetBuilder<DistanceCyclingBuilder.CyclingItem> distanceCyclingBuilder,
            ISheetBuilder<MassBuilder.MassItem> massBuilder,
            ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem> bodyFatBuilder)
        {
            _records = records;
            _workouts = workouts;
            _zone = zone;

            _stepBuilder = stepBuilder;
            _cyclingBuilder = cyclingBuilder;
            _runningBuilder = runningBuilder;
            _walkingBuilder = walkingBuilder;
            _strengthBuilder = strengthBuilder;
            _hiitBuilder = hiitBuilder;
            _distanceCyclingBuilder = distanceCyclingBuilder;
            _massBuilder = massBuilder;
            _bodyFatBuilder = bodyFatBuilder;
        }

        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            var recordMonths = _records.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.InZone(_zone).Year, s.StartDate.InZone(_zone).Month })
                .Select(g => g.Key);

            var workoutMonths = _workouts.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.InZone(_zone).Year, s.StartDate.InZone(_zone).Month })
                .Select(g => g.Key);

            var healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => new DatedItem(m.Year, m.Month).Date);

            var stepsByMonth = _stepBuilder.BuildSummary();
            var cyclingWorkouts = _cyclingBuilder.BuildSummary();
            var cyclingDistances = _distanceCyclingBuilder.BuildSummary();
            var stregthTrainings = _strengthBuilder.BuildSummary();
            var hiits = _hiitBuilder.BuildSummary();
            var runnings = _runningBuilder.BuildSummary();
            var walkings = _walkingBuilder.BuildSummary();
            var masses = _massBuilder.BuildSummary();
            var bodyFats = _bodyFatBuilder.BuildSummary();

            var dataByMonth = from month in healthMonths
                      join steps in stepsByMonth on month equals steps.Date into tmpSteps
                      join wCycling in cyclingWorkouts on month equals wCycling.Date into tmpWCycling
                      join rCycling in cyclingDistances on month equals rCycling.Date into tmpRCycling
                      join strength in stregthTrainings on month equals strength.Date into tmpStrength
                      join hiit in hiits on month equals hiit.Date into tmpHiit
                      join running in runnings on month equals running.Date into tmpRunning
                      join walking in walkings on month equals walking.Date into tmpWalking
                      join mass in masses on month equals mass.Date into tmpMasses
                      join bodyFat in bodyFats on month equals bodyFat.Date into tmpBodyFats
                      from steps in tmpSteps.DefaultIfEmpty()
                      from wCycling in tmpWCycling.DefaultIfEmpty()
                      from rCycling in tmpRCycling.DefaultIfEmpty()
                      from strength in tmpStrength.DefaultIfEmpty()
                      from hiit in tmpHiit.DefaultIfEmpty()
                      from running in tmpRunning.DefaultIfEmpty()
                      from walking in tmpWalking.DefaultIfEmpty()
                      from mass in tmpMasses.DefaultIfEmpty()
                      from bodyFat in tmpBodyFats.DefaultIfEmpty()
                      orderby month descending
                      select new
                      {
                          month = month.ToDateTimeUnspecified(),
                          steps?.Steps,
                          AverageMass = mass?.Mass,
                          AverageBodyFatPct = bodyFat?.BodyFatPercentage,
                          cyclingWorkoutDistance = wCycling?.Distance,
                          cyclingWorkoutMinutes = wCycling?.Duration,
                          distanceCyclingDistance = rCycling?.Distance,
                          strengthMinutes = strength?.Duration,
                          hiitMinutes = hiit?.Duration,
                          runningDistance = running?.Distance,
                          runningDuration = running?.Duration,
                          walkingDistance = walking?.Distance,
                          walkingDuration = walking?.Duration,
                      };

            return dataByMonth;
        }
    }
}
