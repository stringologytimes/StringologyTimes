data <- read.csv("./data/paper_statistics_for_each_year.csv", fileEncoding = "utf8", header = T)
cols <- c("black", "red", "blue", "green", "purple")
ltys <- c(1, 2, 4, 5, 6)
pchs <- c(1, 4, 8, 9, 10)

xmin <- data[1,1]
xmax <- rev(data[,1])[1]

#png('plot.png')
#dev.new(width=25, height=25)
png("docs/output/paper_statistics_for_each_year.png", width = 800, height = 500) 
#par(pin = c(20,10))
plot(0, 0, main="Number of registered papers per publication year",type = "n", xlim = c(xmin, xmax), ylim = c(0, 500), xlab = names(data)[1], ylab = "Count", yaxp=c(0,500,10), xaxp=c(1975,2025,10))
#axis(1, at=c(1980, 1985, 1990),labels=c("aaa", "bbb", "ccc"))

for (i in 2:ncol(data)) {
  points(xmin:xmax, data[, i], pch = pchs[i-1], col = cols[i-1])
  lines(xmin:xmax,data[, i], lty = ltys[i-1], col = cols[i-1])
}

labels <- colnames(data[,2:5])
legend("topleft", legend = labels, col = cols, pch = pchs, lty = ltys)

#plot(data[,1],data[,2], type="l", col="black")
#points(data[,1],data[,3],type="l", col="red")
#points(data[,1],data[,4],type="l", col="blue")
#points(data[,1],data[,5],type="l", col="green")
#
#legend("topleft", names(data)[2:5],
#lwd=c(5,2,3,1,2,2,2,2),
#col=c("black","brown","black","black","blue","green","orange","pink"),
#lty=c("solid","solid","solid","solid","solid","dashed","dotted","dotdash"))