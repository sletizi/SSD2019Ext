# ARIMA, orders data series
import os,sys, io, base64
os.getcwd()
local_path = sys.argv[1]
#local_path = "C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\estensioneProgetto\\SSD2019\\SSD2019\\python_scripts" 
os.chdir(local_path) 

import pandas as pd
import pymssql
import numpy as np
from sqlalchemy import create_engine

from statsmodels.graphics.tsaplots import plot_acf, plot_pacf
import matplotlib.pyplot as plt
plt.rcParams.update({'figure.figsize':(6,4), 'figure.dpi':120})

# ---------------------------- read from sqlite database
def load_orders(customers):
	
    SQL = "SELECT time,quant FROM ordini WHERE customer IN ({})".format(customers)
    
    host = "137.204.72.73"
    username = "studSSD"
    password = "studSSD"
    db = "studenti"
    engine = pymssql.connect(host, username, password, db)						

    df_allorders = pd.read_sql_query(SQL, engine)

    return df_allorders

# ------------------------------ Accuracy metrics
def forecast_accuracy(forecast, actual):
    mape = np.mean(np.abs(forecast - actual)/np.abs(actual))  # MAPE
    me = np.mean(forecast - actual)             # ME
    mae = np.mean(np.abs(forecast - actual))    # MAE
    mpe = np.mean((forecast - actual)/actual)   # MPE
    rmse = np.mean((forecast - actual)**2)**.5  # RMSE
    corr = np.corrcoef(forecast, actual)[0,1]   # corr
    mins = np.amin(np.hstack([forecast[:,None], 
                              actual[:,None]]), axis=1)
    maxs = np.amax(np.hstack([forecast[:,None], 
                              actual[:,None]]), axis=1)
    minmax = 1 - np.mean(mins/maxs)             # minmax
    return({'mape':mape, 'me':me, 'mae': mae, 
            'mpe': mpe, 'rmse':rmse, 
            'corr':corr, 'minmax':minmax})

#read time series into dataframe df
#df = pd.read_csv('customer12.csv', header=0, names = ['cust12'], index_col=0)
customers = sys.argv[2]
#customers = "'cust1'"

df = load_orders(customers)

# !pip3 install pyramid-arima
import pmdarima.arima as pm
from statsmodels.tsa.arima_model import ARIMA

def load_stock_data(db, tickers, start_date, end_date):

	"""
	Loads the stock data for the specified ticker symbols, and for the specified date range.
	:param db: Full path to database with stock data.
	:param tickers: A list with ticker symbols.
	:param start_date: The start date.
	:param end_date: The start date.
	:return: A list of time-indexed dataframe, one for each ticker, ordered by date.
	"""

	SQL = "SELECT * FROM Quotes WHERE TICKER IN ({}) AND Date >= '{}' AND Date <= '{}'"\
		.format(tickers, start_date, end_date)
            
	engine = create_engine('sqlite:///' + db)

	df_all = pd.read_sql(SQL, engine, index_col='Date', parse_dates='Date')
	df_all = df_all.round(2)

	result = []

	for ticker in tickers.split(","):
		df_ticker = df_all.query("Ticker == " + ticker)
		result.append(df_ticker)

	return result

#comunica con il db
def load_orders(customers):

    SQL = "SELECT * FROM ordini WHERE customer IN ({})".format(customers)
    print(SQL)
    host = "137.204.72.73"
    username = "studSSD"
    password = "studSSD"
    db = "studenti"
    engine = pymssql.connect(host, username, password, db)						

    #df_allorders = pd.read_sql_query(SQL, engine)
    df_allorders = pd.read_sql(SQL, engine, index_col='id')

    result = [] 

    for cust in customers.split(","):
        df_order = df_allorders.query("customer == " + cust)
        result.append(df_order)
    return result
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



def print_figure(fig):
	"""
	Converts a figure (as created e.g. with matplotlib or seaborn) to a png image and this 
	png subsequently to a base64-string, then prints the resulting string to the console.
	"""
	
	buf = io.BytesIO()
	fig.savefig(buf, format='png')
	print(base64.b64encode(buf.getbuffer()))

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

# Custom colormap that is used with line charts
COLOR_MAP = [
	'blue', 'orange', 'green', 'red', 'purple', 'brown', 'pink', 'gray', 'olive', 'cyan',
	'darkblue', 'darkorange', 'darkgreen', 'darkred', 'rebeccapurple', 'darkslategray', 
	'mediumvioletred', 'dimgray', 'seagreen', 'darkcyan', 'deepskyblue', 'yellow', 
	'lightgreen', 'lightcoral', 'plum', 'lightslategrey', 'lightpink', 'lightgray', 
	'lime', 'cadetblue'
	]

# Forecast next 3 months
n_forecast = 3

train = df.quant[0:-n_forecast]
test  = df.quant[-n_forecast:]

# Seasonal - fit stepwise auto-ARIMA, returns an ARIMA model
smodel = pm.auto_arima(train, start_p=1, start_q=1,
                         test='adf',
                         max_p=3, max_q=3, m=12,
                         start_P=0, seasonal=True,
                         d=None, D=1, trace=True,
                         error_action='ignore',  
                         suppress_warnings=True, 
                         stepwise=True)

# Predictions of y values based on "model", aka fitted values
yhat = smodel.predict_in_sample(start=0, end=len(train))

forecasts, confint = smodel.predict(n_periods=n_forecast, return_conf_int=True)
index_forecasts = pd.Series(range(df.index[-1]+1-n_forecast, df.index[-1]+1))

metrics = forecast_accuracy(forecasts, df.quant[-n_forecast-1:-1])
print("MAPE = {:.2f}".format(metrics['mape']) )

for i in range(0,n_forecast):
	print("Actual {:.2f} forecast {:.2f}".format(test[len(train)+i],forecasts[i]))

# make series for plotting purpose
fitted_series = pd.Series(forecasts, index=index_forecasts)
lower_series = pd.Series(confint[:, 0], index=index_forecasts)
upper_series = pd.Series(confint[:, 1], index=index_forecasts)

# Plot
plt.plot(df)
plt.plot(yhat,color='brown')
plt.plot(fitted_series, color='darkgreen')
plt.fill_between(lower_series.index, 
                 lower_series, 
                 upper_series, 
                 color='k', alpha=.15)

plt.title("SARIMA - Final Forecast of {}".format(customers))
#plt.show()

# Finally, print the chart as base64 string to the console.
print_figure(plt.gcf())
