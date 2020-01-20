# Outputs a line chart of the time series of the orders of the selected customers.
# To be called via command line / C# (PythonRunner).
import sys #usata per passare parametri
import os
local_path = sys.argv[1] #nell'array di argv ci saranno i parametri
#local_path = "C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\estensioneProgetto\\SSD2019\\SSD2019\\python_scripts" 
os.chdir(local_path) 


import pandas as pd
import matplotlib.pyplot as plt
from matplotlib import style
from cycler import cycler #serve x fare i grafici di colori diversi generando una seq di valori diversi presi da un array
import warnings #serve x filtrare l'output con warning k non mi servono altrimenti lo dovrei fare da c#

from sqlalchemy import create_engine
import pandas as pd
import pymssql
import io, base64


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

# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

# Suppress all kinds of warnings (this would lead to an exception on the client side).
warnings.simplefilter("ignore")

# Preconfig plotting style, line colors and chart size.
style.use('ggplot')
plt.figure(figsize=(7, 5))
plt.rc('axes', prop_cycle=(cycler('color', COLOR_MAP)))

#db_path    = "C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\ordiniMI2018.sqlite"
customers = sys.argv[2]
#customers  =  "'cust4','cust12','cust13','cust50','cust29','cust11','cust20','cust22','cust1','cust6','cust30','cust46'"

# Get the orders from the database.
dfs = load_orders(customers)

# Draw a line to the chart for every single customer.
for df in dfs:
	x = df['quant']
	y = df['time']
	plt.plot(y,x,linewidth=1)

plt.xlabel('Mesi')
plt.ylabel('Quant')

# Finally, print the chart as base64 string to the console.
print_figure(plt.gcf()) #genera una stringa che contiene la codifica binaria del grafico, è ciò k viene 
#esportato e letto da c# di cui poi farà il rendering


