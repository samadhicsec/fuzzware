﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    public class CTFCView : DependencyObject
    {
        ComplexTypeFuzzerConfig oCTFC;

        #region Dependency Properties definition
        public static DependencyProperty OccurrenceFuzzerProperty;
        public static DependencyProperty OrderFuzzerProperty;

        static CTFCView()
        {
            OccurrenceFuzzerProperty = DependencyProperty.Register("DefaultStringFuzzer", typeof(OccurrenceFuzzerView), typeof(CTFCView));

            OrderFuzzerProperty = DependencyProperty.Register("CustomStringFuzzers", typeof(OrderFuzzerView), typeof(CTFCView));
        }

        public OccurrenceFuzzerView OccurrenceFuzzer
        {
            get { return (OccurrenceFuzzerView)GetValue(OccurrenceFuzzerProperty); }
            set { SetValue(OccurrenceFuzzerProperty, value); }
        }

        public OrderFuzzerView OrderFuzzer
        {
            get { return (OrderFuzzerView)GetValue(OrderFuzzerProperty); }
            set { SetValue(OrderFuzzerProperty, value); }
        }

        #endregion

        public CTFCView(ComplexTypeFuzzerConfig ComplexTypeConfig)
        {
            oCTFC = ComplexTypeConfig;

            CreateOccurrenceFuzzerView();
            CreateOrderFuzzerView();
        }

        private void CreateOccurrenceFuzzerView()
        {
            OccurrenceFuzzer = new OccurrenceFuzzerView(oCTFC.OccurranceFuzzingCount);
        }

        private void CreateOrderFuzzerView()
        {
            OrderFuzzer = new OrderFuzzerView(oCTFC.OrderFuzzingCount, oCTFC.OrderFuzzingCountSpecified);
        }

        public ComplexTypeFuzzerConfig ComplexTypeConfig
        {
            get
            {
                OccurrenceFuzzer.SaveOccurrenceFuzzerView(oCTFC);
                OrderFuzzer.SaveOrderFuzzerView(oCTFC);
                return oCTFC;
            }
        }
    }
}
