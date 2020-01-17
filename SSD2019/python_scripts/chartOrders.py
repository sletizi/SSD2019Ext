# Outputs a line chart of the time series of the orders of the selected customers.
# To be called via command line / C# (PythonRunner).
import sys #usata per passare parametri
import os
local_path = sys.argv[1] #nell'array di argv ci saranno i parametri
#local_path = 'C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\progetto\\Form2\\Form2\\[python_scripts]'
os.chdir(local_path) 

import common #import del file che chiama il common.py, utile x non fare tutto in un unico file
import pandas as pd
import matplotlib.pyplot as plt
from matplotlib import style
from cycler import cycler #serve x fare i grafici di colori diversi generando una seq di valori diversi presi da un array
import warnings #serve x filtrare l'output con warning k non mi servono altrimenti lo dovrei fare da c#

# Suppress all kinds of warnings (this would lead to an exception on the client side).
warnings.simplefilter("ignore")

# Preconfig plotting style, line colors and chart size.
style.use('ggplot')
plt.figure(figsize=(7, 5))
plt.rc('axes', prop_cycle=(cycler('color', common.COLOR_MAP)))

# parse command line arguments
db_path = sys.argv[2]
#db_path    = "C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\ordiniMI2018.sqlite"
customers = sys.argv[3]
#customers  =  "'cust4','cust12','cust13','cust50','cust29','cust11','cust20','cust22','cust1','cust6','cust30','cust46'"

# Get the orders from the database.
dfs = common.load_orders(db_path, customers)

# Draw a line to the chart for every single customer.
for df in dfs:
	x = df['quant']
	y = df['time']
	plt.plot(y,x,linewidth=1)

plt.xlabel('Mesi')
plt.ylabel('Quant')

# Finally, print the chart as base64 string to the console.
common.print_figure(plt.gcf()) #genera una stringa che contiene la codifica binaria del grafico, è ciò k viene 
#esportato e letto da c# di cui poi farà il rendering


