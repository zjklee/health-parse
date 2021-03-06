﻿using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : IRawSheetBuilder<(int Year, int Month)>
    {
        private readonly IEnumerable<(int Year, int Month)> _healthMonths;
        private readonly IEnumerable<Column<(int Year, int Month)>> _columns;

        public SummaryBuilder(
            IEnumerable<Record> records,
            IEnumerable<Workout> workouts,
            DateTimeZone zone,
            StepBuilder stepBuilder,
            GeneralRecordsBuilder generalRecordsBuilder,
            HealthMarkersBuilder healthMarkersBuilder,
            NutritionBuilder nutritionBuilder,
            CyclingWorkoutBuilder cyclingBuilder,
            PlayWorkoutBuilder playBuilder,
            EllipticalWorkoutBuilder ellipticalBuilder,
            RunningWorkoutBuilder runningBuilder,
            WalkingWorkoutBuilder walkingBuilder,
            StrengthTrainingBuilder strengthBuilder,
            HiitBuilder hiitBuilder,
            DistanceCyclingBuilder distanceCyclingBuilder,
            MassBuilder massBuilder,
            BodyFatPercentageBuilder bodyFatBuilder)
        {
            var recordMonths = records
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            var workoutMonths = workouts
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            _healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => (Year: m.Year, Month: m.Month));

            _columns = Enumerable.Empty<Column<(int Year, int Month)>>()
                .Concat(stepBuilder.BuildSummary())
                .Concat(bodyFatBuilder.BuildSummary())
                .Concat(generalRecordsBuilder.BuildSummary())
                .Concat(healthMarkersBuilder.BuildSummary())
                .Concat(nutritionBuilder.BuildSummary())
                .Concat(massBuilder.BuildSummary())
                .Concat(distanceCyclingBuilder.BuildSummary())
                .Concat(cyclingBuilder.BuildSummary())
                .Concat(playBuilder.BuildSummary())
                .Concat(ellipticalBuilder.BuildSummary())
                .Concat(walkingBuilder.BuildSummary())
                .Concat(runningBuilder.BuildSummary())
                .Concat(strengthBuilder.BuildSummary())
                .Concat(hiitBuilder.BuildSummary())
                ;
        }

        public Dataset<(int Year, int Month)> BuildRawSheet()
        {
            return new Dataset<(int Year, int Month)>(
                new KeyColumn<(int Year, int Month)>(_healthMonths) { Header = ColumnNames.Month() },
                _columns.ToArray());
        }
    }
}
